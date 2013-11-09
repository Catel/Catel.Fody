// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatelPropertyWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Properties
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

    public class CatelPropertyWeaver
    {
        private readonly ModuleWeaver _moduleWeaver;
        private readonly CatelTypeProperty _propertyData;
        private readonly CatelType _catelType;

        private readonly Dictionary<string, string> _cachedFieldNames = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _cachedFieldInitializerNames = new Dictionary<string, string>();

        public CatelPropertyWeaver(ModuleWeaver moduleWeaver, CatelTypeProperty propertyData, CatelType catelType)
        {
            _moduleWeaver = moduleWeaver;
            _propertyData = propertyData;
            _catelType = catelType;
        }

        public void Execute()
        {
            var property = _propertyData.PropertyDefinition;
            if (property == null)
            {
                _moduleWeaver.LogWarning("Skipping an unknown property because it has no property definition");
                return;
            }

            if (!HasBackingField(property))
            {
                _moduleWeaver.LogInfo(string.Format("\t\tSkipping '{0}' because it has no backing field", property.Name));
                return;
            }

            if (ImplementsICommand(property))
            {
                _moduleWeaver.LogInfo(string.Format("\t\tSkipping '{0}' because it implements ICommand", property.Name));
                return;
            }

            _moduleWeaver.LogInfo("\t\t" + property.Name);

            EnsureStaticConstructor(property.DeclaringType);

            AddChangeNotificationHandlerField(property, _propertyData);

            AddPropertyFieldDefinition(property);
            AddPropertyRegistration(property, _propertyData);

            AddGetValueCall(property);
            AddSetValueCall(property);

            RemoveBackingField(property);
        }

        private string GetChangeNotificationHandlerFieldName(PropertyDefinition property)
        {
            string key = string.Format("{0}|{1}", property.DeclaringType.FullName, property.Name);
            if (_cachedFieldNames.ContainsKey(key))
            {
                return _cachedFieldNames[key];
            }

            int counter = 2; // start at 2
            while (true)
            {
                string fieldName = string.Format("CS$<>9__CachedAnonymousMethodDelegate{0}", counter);
                if (GetField(property.DeclaringType, fieldName) == null)
                {
                    _cachedFieldNames[key] = fieldName;
                    return fieldName;
                }

                counter++;
            }
        }

        private string GetChangeNotificationHandlerConstructorName(PropertyDefinition property)
        {
            string key = string.Format("{0}|{1}", property.DeclaringType.FullName, property.Name);
            if (_cachedFieldInitializerNames.ContainsKey(key))
            {
                return _cachedFieldInitializerNames[key];
            }

            int counter = 0; // start at 0
            while (true)
            {
                string methodName = string.Format("<.cctor>b__{0}", counter);
                if (GetMethod(property.DeclaringType, methodName) == null)
                {
                    _cachedFieldInitializerNames[key] = methodName;
                    return methodName;
                }

                counter++;
            }
        }

        private void EnsureStaticConstructor(TypeDefinition type)
        {
            var staticConstructor = type.Constructor(true);
            if (staticConstructor == null)
            {
                _moduleWeaver.LogInfo(string.Format("\t\t\t{0} - adding static constructor", type.Name));

                staticConstructor = new MethodDefinition(".cctor", MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static | MethodAttributes.RTSpecialName,
                                                         type.Module.Import(typeof (void)));

                var body = staticConstructor.Body;
                body.SimplifyMacros();

                body.Instructions.Add(Instruction.Create(OpCodes.Ret));

                body.OptimizeMacros();

                type.Methods.Add(staticConstructor);

                _moduleWeaver.LogInfo(string.Format("\t\t\t{0} - added static constructor", type.Name));
            }
        }

        private void AddChangeNotificationHandlerField(PropertyDefinition property, CatelTypeProperty propertyData)
        {
            if (propertyData.ChangeCallbackReference == null)
            {
                return;
            }

            _moduleWeaver.LogInfo(string.Format("\t\t\t{0} - adding On{0}Changed invocation", property.Name));

            var declaringType = property.DeclaringType;
            string fieldName = GetChangeNotificationHandlerFieldName(property);

            var handlerType = GetEventHandlerAdvancedPropertyChangedEventArgs(property);
            var advancedPropertyChangedEventArgsType = property.Module.FindType("Catel.Core", "Catel.Data.AdvancedPropertyChangedEventArgs");

            //.field private static class [mscorlib]System.EventHandler`1<class [Catel.Core]Catel.Data.AdvancedPropertyChangedEventArgs> CS$<>9__CachedAnonymousMethodDelegate1

            var field = new FieldDefinition(fieldName, FieldAttributes.Private | FieldAttributes.Static, declaringType.Module.Import(handlerType));
            var compilerGeneratedAttribute = property.Module.FindType("mscorlib", "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
            //var compilerGeneratedAttribute = _allTypes.First(x => x.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
            field.CustomAttributes.Add(new CustomAttribute(property.DeclaringType.Module.Import(compilerGeneratedAttribute.Resolve().Constructor(false))));
            declaringType.Fields.Add(field);

            //.method private hidebysig static void <.cctor>b__0(object s, class [Catel.Core]Catel.Data.AdvancedPropertyChangedEventArgs e) cil managed
            //{
            //    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor()
            //    .maxstack 8
            //    L_0000: ldarg.0 
            //    L_0001: castclass Catel.Fody.TestAssembly.ModelBaseTest
            //    L_0006: callvirt instance void Catel.Fody.TestAssembly.ModelBaseTest::OnLastNameChanged()
            //    L_000b: nop 
            //    L_000c: ret 
            //}

            string initializationMethodName = GetChangeNotificationHandlerConstructorName(property);
            var initializationMethod = new MethodDefinition(initializationMethodName,
                                                            MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static, declaringType.Module.Import(typeof (void)));
            initializationMethod.CustomAttributes.Add(new CustomAttribute(property.DeclaringType.Module.Import(compilerGeneratedAttribute.Resolve().Constructor(false))));
            initializationMethod.Parameters.Add(new ParameterDefinition("s", ParameterAttributes.None, declaringType.Module.Import(typeof (object))));
            initializationMethod.Parameters.Add(new ParameterDefinition("e", ParameterAttributes.None, declaringType.Module.Import(advancedPropertyChangedEventArgsType)));

            var body = initializationMethod.Body;
            body.Instructions.Insert(0,
                                     Instruction.Create(OpCodes.Ldarg_0),
                                     Instruction.Create(OpCodes.Castclass, declaringType),
                                     Instruction.Create(OpCodes.Callvirt, propertyData.ChangeCallbackReference),
                                     Instruction.Create(OpCodes.Nop),
                                     Instruction.Create(OpCodes.Ret));

            declaringType.Methods.Add(initializationMethod);
        }

        private void AddPropertyFieldDefinition(PropertyDefinition property)
        {
            string fieldName = string.Format("{0}Property", property.Name);
            var declaringType = property.DeclaringType;

            declaringType.Fields.Add(new FieldDefinition(fieldName, FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly,
                                                         _catelType.PropertyDataType));
        }

        private void AddPropertyRegistration(PropertyDefinition property, CatelTypeProperty propertyData)
        {
            string fieldName = string.Format("{0}Property", property.Name);
            var declaringType = property.DeclaringType;
            var fieldReference = GetField(declaringType, fieldName);

            var staticConstructor = declaringType.Constructor(true);

            var body = staticConstructor.Body;
            body.SimplifyMacros();

            var instructions = body.Instructions;

            var returnInstruction = (from instruction in instructions
                                     where instruction.OpCode == OpCodes.Ret
                                     select instruction).FirstOrDefault();
            var index = (returnInstruction != null) ? instructions.IndexOf(returnInstruction) : instructions.Count;

            //L_0000: ldstr "FullName"
            //L_0005: ldtoken string // not that this is the property type
            //L_000a: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
            //L_000f: ldnull 
            //L_0010: ldnull 
            //L_0011: ldc.i4.1 
            //L_0012: call class [Catel.Core]Catel.Data.PropertyData [Catel.Core]Catel.Data.ModelBase::RegisterProperty(string, class [mscorlib]System.Type, class [mscorlib]System.Func`1<object>, class [mscorlib]System.EventHandler`1<class [Catel.Core]Catel.Data.AdvancedPropertyChangedEventArgs>, bool)
            //L_0017: stsfld class [Catel.Core]Catel.Data.PropertyData Catel.Fody.TestAssembly.ViewModelBaseTest::FullNameProperty

            var getTypeFromHandle = property.Module.GetMethod("GetTypeFromHandle");
            var importedGetTypeFromHandle = property.Module.Import(getTypeFromHandle);

            var instructionsToInsert = new List<Instruction>();
            instructionsToInsert.AddRange(new[]
                                              {
                                                  Instruction.Create(OpCodes.Ldstr, property.Name),
                                                  Instruction.Create(OpCodes.Ldtoken, property.PropertyType),
                                                  Instruction.Create(OpCodes.Call, importedGetTypeFromHandle),
                                                  Instruction.Create(OpCodes.Ldnull)
                                              });

            if (propertyData.ChangeCallbackReference != null)
            {
                //L_0040: ldsfld class [mscorlib]System.EventHandler`1<class [Catel.Core]Catel.Data.AdvancedPropertyChangedEventArgs> Catel.Fody.TestAssembly.ModelBaseTest::CS$<>9__CachedAnonymousMethodDelegate1
                //L_0045: brtrue.s L_005a
                //L_0047: ldnull 
                //L_0048: ldftn void Catel.Fody.TestAssembly.ModelBaseTest::<.cctor>b__0(object, class [Catel.Core]Catel.Data.AdvancedPropertyChangedEventArgs)
                //L_004e: newobj instance void [mscorlib]System.EventHandler`1<class [Catel.Core]Catel.Data.AdvancedPropertyChangedEventArgs>::.ctor(object, native int)
                //L_0053: stsfld class [mscorlib]System.EventHandler`1<class [Catel.Core]Catel.Data.AdvancedPropertyChangedEventArgs> Catel.Fody.TestAssembly.ModelBaseTest::CS$<>9__CachedAnonymousMethodDelegate1
                //L_0058: br.s L_005a
                //L_005a: ldsfld class [mscorlib]System.EventHandler`1<class [Catel.Core]Catel.Data.AdvancedPropertyChangedEventArgs> Catel.Fody.TestAssembly.ModelBaseTest::CS$<>9__CachedAnonymousMethodDelegate1

                string handlerFieldName = GetChangeNotificationHandlerFieldName(property);
                string handlerConstructorFieldName = GetChangeNotificationHandlerConstructorName(property);

                var handlerField = GetField(property.DeclaringType, handlerFieldName);
                var handlerConstructor = GetMethod(property.DeclaringType, handlerConstructorFieldName);
                var handlerType = GetEventHandlerAdvancedPropertyChangedEventArgs(property);
                var importedHandlerType = handlerType.Resolve();

                var advancedPropertyChangedEventArgsType = property.Module.FindType("Catel.Core", "Catel.Data.AdvancedPropertyChangedEventArgs");
                var handlerTypeConstructor = declaringType.Module.Import(importedHandlerType.Constructor(false));
                var genericConstructor = handlerTypeConstructor.MakeHostInstanceGeneric(declaringType.Module.Import(advancedPropertyChangedEventArgsType));

                var finalInstruction = Instruction.Create(OpCodes.Ldsfld, handlerField);

                instructionsToInsert.AddRange(new[]
                                                  {
                                                      Instruction.Create(OpCodes.Ldsfld, handlerField),
                                                      Instruction.Create(OpCodes.Brtrue_S, finalInstruction),
                                                      Instruction.Create(OpCodes.Ldnull),
                                                      Instruction.Create(OpCodes.Ldftn, handlerConstructor),
                                                      Instruction.Create(OpCodes.Newobj, genericConstructor),
                                                      Instruction.Create(OpCodes.Stsfld, handlerField),
                                                      Instruction.Create(OpCodes.Br_S, finalInstruction),
                                                      finalInstruction
                                                  });
            }
            else
            {
                instructionsToInsert.Add(Instruction.Create(OpCodes.Ldnull));
            }

            var parameters = _catelType.RegisterPropertyInvoker.Parameters.Reverse().ToList();
            for (int i = 0; i < parameters.Count; i++)
            {
                var parameterType = parameters[i];
                if (string.CompareOrdinal(parameterType.ParameterType.FullName, _moduleWeaver.ModuleDefinition.TypeSystem.Boolean.FullName) != 0)
                {
                    break;
                }

                instructionsToInsert.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            }

            instructionsToInsert.AddRange(new[]
                                              {
                                                  Instruction.Create(OpCodes.Call, _catelType.RegisterPropertyInvoker),
                                                  Instruction.Create(OpCodes.Stsfld, fieldReference)
                                              });

            instructions.Insert(index, instructionsToInsert);

            body.OptimizeMacros();
        }

        private int AddGetValueCall(PropertyDefinition property)
        {
            _moduleWeaver.LogInfo(string.Format("\t\t\t{0} - adding GetValue call", property.Name));

            var genericGetValue = new GenericInstanceMethod(_catelType.GetValueInvoker);

            foreach (var genericParameter in _catelType.GetValueInvoker.GenericParameters)
            {
                genericGetValue.GenericParameters.Add(genericParameter);
            }

            genericGetValue.GenericArguments.Add(property.PropertyType);

            var body = property.GetMethod.Body;
            body.SimplifyMacros();

            var instructions = body.Instructions;
            instructions.Clear();

            var finalIndex = instructions.Insert(0,
                                                 Instruction.Create(OpCodes.Nop),
                                                 Instruction.Create(OpCodes.Ldarg_0),
                                                 Instruction.Create(OpCodes.Ldstr, property.Name),
                                                 Instruction.Create(OpCodes.Call, genericGetValue),
                                                 Instruction.Create(OpCodes.Ret));

            body.OptimizeMacros();

            return finalIndex;
        }

        private int AddSetValueCall(PropertyDefinition property)
        {
            _moduleWeaver.LogInfo(string.Format("\t\t\t{0} - adding SetValue call", property.Name));

            //string fieldName = string.Format("{0}Property", property.Name);
            //var declaringType = property.DeclaringType;
            //var fieldReference = GetField(declaringType, fieldName);

            // Writes SetValue(PropertyData propertyName, object value)

            var body = property.SetMethod.Body;
            body.SimplifyMacros();

            var instructions = body.Instructions;
            instructions.Clear();

            var instructionsToAdd = new List<Instruction>();
            instructionsToAdd.AddRange(new[]
                                           {
                                               Instruction.Create(OpCodes.Nop),
                                               Instruction.Create(OpCodes.Ldarg_0),
                                               Instruction.Create(OpCodes.Ldstr, property.Name),
                                               Instruction.Create(OpCodes.Ldarg_1)
                                           });

            if (property.PropertyType.IsValueType)
            {
                instructionsToAdd.Add(Instruction.Create(OpCodes.Box, property.PropertyType));
            }

            instructionsToAdd.AddRange(new[]
                                           {
                                               Instruction.Create(OpCodes.Call, _catelType.SetValueInvoker),
                                               Instruction.Create(OpCodes.Nop),
                                               Instruction.Create(OpCodes.Ret)
                                           });

            var finalIndex = instructions.Insert(0, instructionsToAdd.ToArray());

            body.OptimizeMacros();

            return finalIndex;
        }

        private string GetBackingFieldName(PropertyDefinition property)
        {
            return string.Format("<{0}>k__BackingField", property.Name);
        }

        private GenericInstanceType GetEventHandlerAdvancedPropertyChangedEventArgs(PropertyDefinition property)
        {
            var genericHandlerType = property.Module.FindType("mscorlib", "System.EventHandler`1");
            if (genericHandlerType == null)
            {
                _moduleWeaver.LogError("Expected to find EventHandler<T>, but type was not  found");
                return null;
            }

            var advancedPropertyChangedEventArgsType = property.Module.FindType("Catel.Core", "Catel.Data.AdvancedPropertyChangedEventArgs");

            var handlerType = new GenericInstanceType(genericHandlerType);
            handlerType.GenericArguments.Add(advancedPropertyChangedEventArgsType);

            return handlerType;
        }

        private bool HasBackingField(PropertyDefinition property)
        {
            var fieldName = GetBackingFieldName(property);

            var field = GetField(property.DeclaringType, fieldName);
            return (field != null);
        }

        private bool ImplementsICommand(PropertyDefinition property)
        {
            var commandInterfaces = new List<string>(new[] {"ICommand", "ICatelCommand"});

            var resolvedType = property.PropertyType.Resolve();
            if (resolvedType == null)
            {
                _moduleWeaver.LogWarning(string.Format("Could not resolve type '{0}'", property.PropertyType));
                return false;
            }

            if (resolvedType.IsInterface)
            {
                return commandInterfaces.Any(x => resolvedType.Name.Contains(x));
            }

            var baseTypes = resolvedType.GetBaseTypes(true);
            foreach (var baseType in baseTypes)
            {
                if (commandInterfaces.Any(x => baseType.Name.Contains(x)))
                {
                    return true;
                }
            }

            return false;
        }

        private void RemoveBackingField(PropertyDefinition property)
        {
            var fieldName = GetBackingFieldName(property);

            var field = GetField(property.DeclaringType, fieldName);
            if (field != null)
            {
                property.DeclaringType.Fields.Remove(field);
            }
        }

        private static FieldDefinition GetField(TypeDefinition declaringType, string fieldName)
        {
            return (from field in declaringType.Fields
                    where field.Name == fieldName
                    select field).FirstOrDefault();
        }

        private static MethodDefinition GetMethod(TypeDefinition declaringType, string methodName)
        {
            return (from method in declaringType.Methods
                    where method.Name == methodName
                    select method).FirstOrDefault();
        }
    }
}