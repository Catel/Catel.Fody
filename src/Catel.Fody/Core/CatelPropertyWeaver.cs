// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatelPropertyWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

    public class CatelPropertyWeaver
    {
        #region Fields
        private static readonly Dictionary<string, string> _cachedFieldInitializerNames = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> _cachedFieldNames = new Dictionary<string, string>();

        private readonly CatelType _catelType;
        private readonly CatelTypeProperty _propertyData;
        #endregion

        #region Constructors
        public CatelPropertyWeaver(CatelType catelType, CatelTypeProperty propertyData)
        {
            _catelType = catelType;
            _propertyData = propertyData;
        }
        #endregion

        #region Methods
        public void Execute(bool force = false)
        {
            var property = _propertyData.PropertyDefinition;
            if (property == null)
            {
                FodyEnvironment.LogWarning("Skipping an unknown property because it has no property definition");
                return;
            }

            if (!force && !HasBackingField(property))
            {
                FodyEnvironment.LogInfo(string.Format("\t\tSkipping '{0}' because it has no backing field", property.Name));
                return;
            }

            if (ImplementsICommand(property))
            {
                FodyEnvironment.LogInfo(string.Format("\t\tSkipping '{0}' because it implements ICommand", property.Name));
                return;
            }

            FodyEnvironment.LogInfo("\t\t" + property.Name);

            EnsureStaticConstructor(property.DeclaringType);

            AddChangeNotificationHandlerField(property, _propertyData);

            var fieldDefinition = AddPropertyFieldDefinition(property);
            AddPropertyRegistration(property, _propertyData);

            var fieldReference = GetFieldReference(property.DeclaringType, fieldDefinition.Name, true);

            AddGetValueCall(property, fieldReference);
            AddSetValueCall(property, fieldReference, _propertyData.IsReadOnly);

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
                if (GetFieldDefinition(property.DeclaringType, fieldName, false) == null)
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
                if (GetMethodReference(property.DeclaringType, methodName, false) == null)
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
                FodyEnvironment.LogInfo(string.Format("\t\t\t{0} - adding static constructor", type.Name));

                staticConstructor = new MethodDefinition(".cctor", MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static | MethodAttributes.RTSpecialName,
                    type.Module.Import(typeof(void)));

                var body = staticConstructor.Body;
                body.SimplifyMacros();

                body.Instructions.Add(Instruction.Create(OpCodes.Ret));

                body.OptimizeMacros();

                type.Methods.Add(staticConstructor);

                FodyEnvironment.LogInfo(string.Format("\t\t\t{0} - added static constructor", type.Name));
            }
        }

        private void AddChangeNotificationHandlerField(PropertyDefinition property, CatelTypeProperty propertyData)
        {
            if (propertyData.ChangeCallbackReference == null)
            {
                return;
            }

            FodyEnvironment.LogInfo(string.Format("\t\t\t{0} - adding On{0}Changed invocation", property.Name));

            var declaringType = property.DeclaringType;
            string fieldName = GetChangeNotificationHandlerFieldName(property);

            var handlerType = GetEventHandlerAdvancedPropertyChangedEventArgs(property);
            var advancedPropertyChangedEventArgsType = property.Module.FindType("Catel.Core", "Catel.Data.AdvancedPropertyChangedEventArgs");

            //.field private static class [mscorlib]System.EventHandler`1<class [Catel.Core]Catel.Data.AdvancedPropertyChangedEventArgs> CS$<>9__CachedAnonymousMethodDelegate1

            var field = new FieldDefinition(fieldName, FieldAttributes.Private | FieldAttributes.Static, declaringType.Module.Import(handlerType));
            var compilerGeneratedAttribute = property.Module.FindType("mscorlib", "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
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
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static, declaringType.Module.Import(typeof(void)));
            initializationMethod.CustomAttributes.Add(new CustomAttribute(property.DeclaringType.Module.Import(compilerGeneratedAttribute.Resolve().Constructor(false))));
            initializationMethod.Parameters.Add(new ParameterDefinition("s", ParameterAttributes.None, declaringType.Module.Import(typeof(object))));
            initializationMethod.Parameters.Add(new ParameterDefinition("e", ParameterAttributes.None, declaringType.Module.Import(advancedPropertyChangedEventArgsType)));

            var body = initializationMethod.Body;
            body.Instructions.Insert(0,
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Castclass, declaringType.MakeGenericIfRequired()),
                Instruction.Create(OpCodes.Callvirt, propertyData.ChangeCallbackReference),
                Instruction.Create(OpCodes.Nop),
                Instruction.Create(OpCodes.Ret));

            declaringType.Methods.Add(initializationMethod);
        }

        private FieldDefinition AddPropertyFieldDefinition(PropertyDefinition property)
        {
            string fieldName = string.Format("{0}Property", property.Name);
            var declaringType = property.DeclaringType;

            var fieldDefinition = new FieldDefinition(fieldName, FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly, _catelType.PropertyDataType);

            var compilerGeneratedAttribute = property.Module.FindType("mscorlib", "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
            fieldDefinition.CustomAttributes.Add(new CustomAttribute(property.DeclaringType.Module.Import(compilerGeneratedAttribute.Resolve().Constructor(false))));

            declaringType.Fields.Add(fieldDefinition);

            return fieldDefinition;
        }

        private void AddPropertyRegistration(PropertyDefinition property, CatelTypeProperty propertyData)
        {
            string fieldName = string.Format("{0}Property", property.Name);
            var declaringType = property.DeclaringType;
            var fieldReference = GetFieldReference(declaringType, fieldName, true);

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
                Instruction.Create(OpCodes.Ldtoken, ImportPropertyType(property)),
                Instruction.Create(OpCodes.Call, importedGetTypeFromHandle),
            });

            var resolvedPropertyType = propertyData.PropertyDefinition.PropertyType.Resolve();

            // Default value
            if (propertyData.DefaultValue is string)
            {
                instructionsToInsert.Add(Instruction.Create(OpCodes.Ldstr, (string)propertyData.DefaultValue));
            }
            else if (propertyData.DefaultValue is bool)
            {
                instructionsToInsert.Add(Instruction.Create(OpCodes.Ldc_I4, (bool)propertyData.DefaultValue ? 1 : 0));
            }
            else if (propertyData.DefaultValue is int)
            {
                instructionsToInsert.Add(Instruction.Create(OpCodes.Ldc_I4, (int)propertyData.DefaultValue));
            }
            else if (propertyData.DefaultValue is long)
            {
                if ((long)propertyData.DefaultValue <= int.MaxValue)
                {
                    // Note: don't use Ldc_I8 here, although it is a long
                    instructionsToInsert.Add(Instruction.Create(OpCodes.Ldc_I4, (int)(long)propertyData.DefaultValue));
                    instructionsToInsert.Add(Instruction.Create(OpCodes.Conv_I8));
                }
                else
                {
                    instructionsToInsert.Add(Instruction.Create(OpCodes.Ldc_I8, (long)propertyData.DefaultValue));
                }
            }
            else if (propertyData.DefaultValue is float)
            {
                instructionsToInsert.Add(Instruction.Create(OpCodes.Ldc_R4, (float)propertyData.DefaultValue));
            }
            else if (propertyData.DefaultValue is double)
            {
                instructionsToInsert.Add(Instruction.Create(OpCodes.Ldc_R8, (double)propertyData.DefaultValue));
            }
            else if (resolvedPropertyType.IsEnum && propertyData.DefaultValue != null)
            {
                instructionsToInsert.Add(Instruction.Create(OpCodes.Ldc_I4, (int)((CustomAttributeArgument)propertyData.DefaultValue).Value));
            }
            else
            {
                instructionsToInsert.Add(Instruction.Create(OpCodes.Ldnull));
            }

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

                var handlerField = GetFieldReference(property.DeclaringType, handlerFieldName, true);
                var handlerConstructor = GetMethodReference(property.DeclaringType, handlerConstructorFieldName, true);
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

            var registerPropertyInvoker = (propertyData.DefaultValue == null) ? _catelType.RegisterPropertyWithoutDefaultValueInvoker : _catelType.RegisterPropertyWithDefaultValueInvoker;

            var parameters = registerPropertyInvoker.Parameters.Reverse().ToList();
            for (int i = 0; i < parameters.Count; i++)
            {
                var parameterType = parameters[i];
                if (string.CompareOrdinal(parameterType.ParameterType.FullName, FodyEnvironment.ModuleDefinition.TypeSystem.Boolean.FullName) != 0)
                {
                    break;
                }

                instructionsToInsert.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            }

            // Make call to register property generic
            var genericRegisterProperty = new GenericInstanceMethod(registerPropertyInvoker);
            if (registerPropertyInvoker.HasGenericParameters)
            {
                foreach (var genericParameter in registerPropertyInvoker.GenericParameters)
                {
                    genericRegisterProperty.GenericParameters.Add(genericParameter);
                }

                genericRegisterProperty.GenericArguments.Add(ImportPropertyType(property));
            }

            instructionsToInsert.AddRange(new[]
            {
                Instruction.Create(OpCodes.Call, genericRegisterProperty),
                Instruction.Create(OpCodes.Stsfld, fieldReference)
            });

            instructions.Insert(index, instructionsToInsert);

            body.OptimizeMacros();
        }

        private int AddGetValueCall(PropertyDefinition property, FieldReference fieldReference)
        {
            FodyEnvironment.LogInfo(string.Format("\t\t\t{0} - adding GetValue call", property.Name));

            var genericGetValue = new GenericInstanceMethod(_catelType.GetValueInvoker);

            foreach (var genericParameter in _catelType.GetValueInvoker.GenericParameters)
            {
                genericGetValue.GenericParameters.Add(genericParameter);
            }

            genericGetValue.GenericArguments.Add(ImportPropertyType(property));

            if (property.GetMethod == null)
            {
                var getMethod = new MethodDefinition(string.Format("get_{0}", property.Name), MethodAttributes.Public, ImportPropertyType(property));

                var compilerGeneratedAttribute = property.Module.FindType("mscorlib", "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
                getMethod.CustomAttributes.Add(new CustomAttribute(property.DeclaringType.Module.Import(compilerGeneratedAttribute.Resolve().Constructor(false))));

                property.DeclaringType.Methods.Add(getMethod);
                property.GetMethod = getMethod;
            }

            var body = property.GetMethod.Body;
            body.SimplifyMacros();

            var instructions = body.Instructions;
            instructions.Clear();

            var finalIndex = instructions.Insert(0,
                Instruction.Create(OpCodes.Nop),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldsfld, fieldReference),
                Instruction.Create(OpCodes.Call, genericGetValue),
                Instruction.Create(OpCodes.Ret));

            body.OptimizeMacros();

            return finalIndex;
        }

        private int AddSetValueCall(PropertyDefinition property, FieldReference fieldReference, bool isReadOnly)
        {
            FodyEnvironment.LogInfo(string.Format("\t\t\t{0} - adding SetValue call", property.Name));

            //string fieldName = string.Format("{0}Property", property.Name);
            //var declaringType = property.DeclaringType;
            //var fieldReference = GetField(declaringType, fieldName);

            // Writes SetValue(PropertyData propertyName, object value)

            if (property.SetMethod == null)
            {
                var setMethod = new MethodDefinition(string.Format("set_{0}", property.Name), MethodAttributes.Public, property.DeclaringType.Module.Import(typeof(void)));
                setMethod.Parameters.Add(new ParameterDefinition(ImportPropertyType(property)));

                var compilerGeneratedAttribute = property.Module.FindType("mscorlib", "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
                setMethod.CustomAttributes.Add(new CustomAttribute(property.DeclaringType.Module.Import(compilerGeneratedAttribute.Resolve().Constructor(false))));

                property.DeclaringType.Methods.Add(setMethod);
                property.SetMethod = setMethod;
            }

            var finalSetMethod = property.SetMethod;
            if (isReadOnly)
            {
                finalSetMethod.IsPrivate = true;
                finalSetMethod.IsPublic = false;
            }

            var body = property.SetMethod.Body;
            body.SimplifyMacros();

            var instructions = body.Instructions;
            instructions.Clear();

            var instructionsToAdd = new List<Instruction>();
            instructionsToAdd.AddRange(new[]
            {
                //Instruction.Create(OpCodes.Nop),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldsfld, fieldReference),
                Instruction.Create(OpCodes.Ldarg_1)
            });

            if (property.PropertyType.IsValueType)
            {
                instructionsToAdd.Add(Instruction.Create(OpCodes.Box, ImportPropertyType(property)));
            }

            instructionsToAdd.AddRange(new[]
            {
                Instruction.Create(OpCodes.Call, _catelType.SetValueInvoker),
                //Instruction.Create(OpCodes.Nop),
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
                FodyEnvironment.LogError("Expected to find EventHandler<T>, but type was not  found");
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

            var field = GetFieldReference(property.DeclaringType, fieldName, false);
            return (field != null);
        }

        private bool ImplementsICommand(PropertyDefinition property)
        {
            var commandInterfaces = new List<string>(new[] { "ICommand", "ICatelCommand" });

            var resolvedType = property.PropertyType.Resolve();
            if (resolvedType == null)
            {
                FodyEnvironment.LogWarning(string.Format("Could not resolve type '{0}'", property.PropertyType));
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

            var field = GetFieldDefinition(property.DeclaringType, fieldName, false);
            if (field != null)
            {
                property.DeclaringType.Fields.Remove(field);
            }
        }

        private static TypeReference ImportPropertyType(PropertyDefinition propertyDefinition)
        {
            return propertyDefinition.DeclaringType.Module.Import(propertyDefinition.PropertyType);
        }

        private static FieldDefinition GetFieldDefinition(TypeDefinition declaringType, string fieldName, bool allowGenericResolving)
        {
            var fieldReference = GetFieldReference(declaringType, fieldName, allowGenericResolving);

            return fieldReference != null ? fieldReference.Resolve() : null;
        }

        private static FieldReference GetFieldReference(TypeDefinition declaringType, string fieldName, bool allowGenericResolving)
        {
            var field = (from x in declaringType.Fields
                         where x.Name == fieldName
                         select x).FirstOrDefault();

            if (field == null)
            {
                return null;
            }

            FieldReference fieldReference = field;

            if (declaringType.HasGenericParameters && allowGenericResolving)
            {
                fieldReference = field.MakeGeneric(declaringType);
            }

            return fieldReference;
        }

        private static MethodDefinition GetMethodDefinition(TypeDefinition declaringType, string methodName, bool allowGenericResolving)
        {
            var methodReference = GetMethodReference(declaringType, methodName, allowGenericResolving);

            return methodReference != null ? methodReference.Resolve() : null;
        }

        private static MethodReference GetMethodReference(TypeDefinition declaringType, string methodName, bool allowGenericResolving)
        {
            var method = (from x in declaringType.Methods
                          where x.Name == methodName
                          select x).FirstOrDefault();

            if (method == null)
            {
                return null;
            }

            MethodReference methodReference = method;

            if (declaringType.HasGenericParameters && allowGenericResolving)
            {
                methodReference = method.MakeGeneric(declaringType);
            }

            return methodReference;
        }
        #endregion
    }
}