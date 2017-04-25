// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatelPropertyWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

    public class CatelPropertyWeaver
    {
        #region Fields
        private readonly Dictionary<string, string> _cachedFieldInitializerNames = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _cachedFieldNames = new Dictionary<string, string>();

        private readonly CatelType _catelType;
        private readonly CatelTypeProperty _propertyData;
        private readonly MsCoreReferenceFinder _msCoreReferenceFinder;
        #endregion

        #region Constructors
        public CatelPropertyWeaver(CatelType catelType, CatelTypeProperty propertyData, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            _catelType = catelType;
            _propertyData = propertyData;
            _msCoreReferenceFinder = msCoreReferenceFinder;
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
                FodyEnvironment.LogDebug($"\t\tSkipping '{property.Name}' because it has no backing field");
                return;
            }

            if (ImplementsICommand(property))
            {
                FodyEnvironment.LogDebug($"\t\tSkipping '{property.Name}' because it implements ICommand");
                return;
            }

            FodyEnvironment.LogDebug("\t\t" + property.Name);

            try
            {
                EnsureStaticConstructor(property.DeclaringType);

                AddChangeNotificationHandlerField(property, _propertyData);

                var fieldDefinition = AddPropertyFieldDefinition(property);
                if (!AddPropertyRegistration(property, _propertyData))
                {
                    return;
                }

                var fieldReference = GetFieldReference(property.DeclaringType, fieldDefinition.Name, true);

                AddGetValueCall(property, fieldReference);
                AddSetValueCall(property, fieldReference, _propertyData.IsReadOnly);

                RemoveBackingField(property);
            }
            catch (Exception ex)
            {
                FodyEnvironment.LogError($"\t\tFailed to handle property '{property.DeclaringType.Name}.{property.Name}'\n{ex.Message}\n{ex.StackTrace}");

#if DEBUG
                Debugger.Launch();
#endif
            }
        }


        private string GetChangeNotificationHandlerFieldName(PropertyDefinition property)
        {
            string key = $"{property.DeclaringType.FullName}|{property.Name}";
            if (_cachedFieldNames.ContainsKey(key))
            {
                return _cachedFieldNames[key];
            }

            int counter = 2; // start at 2
            while (true)
            {
                string fieldName = $"CS$<>9__CachedAnonymousMethodDelegate{counter}";
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
            string key = $"{property.DeclaringType.FullName}|{property.Name}";
            if (_cachedFieldInitializerNames.ContainsKey(key))
            {
                return _cachedFieldInitializerNames[key];
            }

            int counter = 0; // start at 0
            while (true)
            {
                string methodName = $"<.cctor>b__{counter}";
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
                FodyEnvironment.LogDebug($"\t\t\t{type.Name} - adding static constructor");

                var voidType = _msCoreReferenceFinder.GetCoreTypeReference("Void");

                staticConstructor = new MethodDefinition(".cctor", MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static | MethodAttributes.RTSpecialName,
                    type.Module.ImportReference(voidType));

                var body = staticConstructor.Body;
                body.SimplifyMacros();

                body.Instructions.Add(Instruction.Create(OpCodes.Ret));

                body.OptimizeMacros();

                type.Methods.Add(staticConstructor);

                FodyEnvironment.LogDebug($"\t\t\t{type.Name} - added static constructor");
            }
        }

        private void AddChangeNotificationHandlerField(PropertyDefinition property, CatelTypeProperty propertyData)
        {
            if (propertyData.ChangeCallbackReference == null)
            {
                return;
            }

            FodyEnvironment.LogDebug($"\t\t\t{property.Name} - adding On{property.Name}Changed invocation");

            var declaringType = property.DeclaringType;
            string fieldName = GetChangeNotificationHandlerFieldName(property);

            var handlerType = GetEventHandlerAdvancedPropertyChangedEventArgs(property);
            var advancedPropertyChangedEventArgsType = property.Module.FindType("Catel.Core", "Catel.Data.AdvancedPropertyChangedEventArgs");

            //.field private static class [mscorlib]System.EventHandler`1<class [Catel.Core]Catel.Data.AdvancedPropertyChangedEventArgs> CS$<>9__CachedAnonymousMethodDelegate1

            var field = new FieldDefinition(fieldName, FieldAttributes.Private | FieldAttributes.Static, declaringType.Module.ImportReference(handlerType));

            declaringType.Fields.Add(field);

            field.MarkAsCompilerGenerated(_msCoreReferenceFinder);

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

            var voidType = _msCoreReferenceFinder.GetCoreTypeReference("Void");
            var objectType = _msCoreReferenceFinder.GetCoreTypeReference("Object");

            string initializationMethodName = GetChangeNotificationHandlerConstructorName(property);
            var initializationMethod = new MethodDefinition(initializationMethodName,
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static, declaringType.Module.ImportReference(voidType));

            initializationMethod.Parameters.Add(new ParameterDefinition("s", ParameterAttributes.None, declaringType.Module.ImportReference(objectType)));
            initializationMethod.Parameters.Add(new ParameterDefinition("e", ParameterAttributes.None, declaringType.Module.ImportReference(advancedPropertyChangedEventArgsType)));

            var body = initializationMethod.Body;
            body.Instructions.Insert(0,
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Castclass, declaringType.MakeGenericIfRequired()),
                Instruction.Create(OpCodes.Callvirt, propertyData.ChangeCallbackReference),
                Instruction.Create(OpCodes.Nop),
                Instruction.Create(OpCodes.Ret));

            declaringType.Methods.Add(initializationMethod);

            initializationMethod.MarkAsCompilerGenerated(_msCoreReferenceFinder);
        }

        private FieldDefinition AddPropertyFieldDefinition(PropertyDefinition property)
        {
            string fieldName = $"{property.Name}Property";
            var declaringType = property.DeclaringType;

            var fieldDefinition = new FieldDefinition(fieldName, FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly, _catelType.PropertyDataType);

            declaringType.Fields.Add(fieldDefinition);

            fieldDefinition.MarkAsCompilerGenerated(_msCoreReferenceFinder);

            return fieldDefinition;
        }

        private bool AddPropertyRegistration(PropertyDefinition property, CatelTypeProperty propertyData)
        {
            var fieldName = $"{property.Name}Property";
            var declaringType = property.DeclaringType;
            var fieldReference = GetFieldReference(declaringType, fieldName, true);
            if (fieldReference == null)
            {
                FodyEnvironment.LogWarning($"\t\tCannot handle property '{_catelType.Name}.{property.Name}' because backing field is not found");
                return false;
            }

            var staticConstructor = declaringType.Constructor(true);

            var body = staticConstructor.Body;
            body.SimplifyMacros();

            var instructions = body.Instructions;

            // Always inject before the first try block, or just before the last return statement
            var instructionToInsert = (from instruction in instructions
                                       where instruction.OpCode == OpCodes.Ret
                                       select instruction).FirstOrDefault();

            var exceptionHandler = body.ExceptionHandlers.FirstOrDefault();
            if (exceptionHandler != null)
            {
                instructionToInsert = exceptionHandler.TryStart;
            }

            var index = (instructionToInsert != null) ? instructions.IndexOf(instructionToInsert) : instructions.Count;

            //L_0000: ldstr "FullName"
            //L_0005: ldtoken string // note that this is the property type
            //L_000a: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
            //L_000f: ldnull 
            //L_0010: ldnull 
            //L_0011: ldc.i4.1 
            //L_0012: call class [Catel.Core]Catel.Data.PropertyData [Catel.Core]Catel.Data.ModelBase::RegisterProperty(string, class [mscorlib]System.Type, class [mscorlib]System.Func`1<object>, class [mscorlib]System.EventHandler`1<class [Catel.Core]Catel.Data.AdvancedPropertyChangedEventArgs>, bool)
            //L_0017: stsfld class [Catel.Core]Catel.Data.PropertyData Catel.Fody.TestAssembly.ViewModelBaseTest::FullNameProperty

            var getTypeFromHandle = property.Module.GetMethodAndImport("GetTypeFromHandle");

            var instructionsToInsert = new List<Instruction>();
            instructionsToInsert.AddRange(new[]
            {
                Instruction.Create(OpCodes.Ldstr, property.Name),
                Instruction.Create(OpCodes.Ldtoken, property.PropertyType.Import()),
                Instruction.Create(OpCodes.Call, getTypeFromHandle),
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
            else if (resolvedPropertyType != null && resolvedPropertyType.IsEnum && propertyData.DefaultValue != null)
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
                var handlerTypeConstructor = declaringType.Module.ImportReference(importedHandlerType.Constructor(false));
                var genericConstructor = handlerTypeConstructor.MakeHostInstanceGeneric(declaringType.Module.ImportReference(advancedPropertyChangedEventArgsType));

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
            var finalRegisterPropertyMethod = registerPropertyInvoker;
            if (registerPropertyInvoker.HasGenericParameters)
            {
                var genericRegisterProperty = new GenericInstanceMethod(registerPropertyInvoker);
                if (registerPropertyInvoker.HasGenericParameters)
                {
                    foreach (var genericParameter in registerPropertyInvoker.GenericParameters)
                    {
                        genericRegisterProperty.GenericParameters.Add(genericParameter);
                    }

                    genericRegisterProperty.GenericArguments.Add(property.PropertyType.Import(true));
                }

                finalRegisterPropertyMethod = genericRegisterProperty;
            }

            instructionsToInsert.AddRange(new[]
            {
                Instruction.Create(OpCodes.Call, finalRegisterPropertyMethod),
                Instruction.Create(OpCodes.Stsfld, fieldReference)
            });

            instructions.Insert(index, instructionsToInsert);

            body.OptimizeMacros();

            return true;
        }

        private int AddGetValueCall(PropertyDefinition property, FieldReference fieldReference)
        {
            FodyEnvironment.LogDebug($"\t\t\t{property.Name} - adding GetValue call");

            var genericGetValue = new GenericInstanceMethod(_catelType.GetValueInvoker);

            foreach (var genericParameter in _catelType.GetValueInvoker.GenericParameters)
            {
                genericGetValue.GenericParameters.Add(genericParameter);
            }

            genericGetValue.GenericArguments.Add(property.PropertyType.Import());

            if (property.GetMethod == null)
            {
                var getMethod = new MethodDefinition($"get_{property.Name}", MethodAttributes.Public, property.PropertyType.Import());

                property.DeclaringType.Methods.Add(getMethod);

                getMethod.MarkAsCompilerGenerated(_msCoreReferenceFinder);

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
            FodyEnvironment.LogDebug($"\t\t\t{property.Name} - adding SetValue call");

            //string fieldName = string.Format("{0}Property", property.Name);
            //var declaringType = property.DeclaringType;
            //var fieldReference = GetField(declaringType, fieldName);

            // Writes SetValue(PropertyData propertyName, object value)

            if (property.SetMethod == null)
            {
                var voidType = _msCoreReferenceFinder.GetCoreTypeReference("Void");

                var setMethod = new MethodDefinition($"set_{property.Name}", MethodAttributes.Public, property.DeclaringType.Module.ImportReference(voidType));
                setMethod.Parameters.Add(new ParameterDefinition(property.PropertyType.Import()));

                property.DeclaringType.Methods.Add(setMethod);

                setMethod.MarkAsCompilerGenerated(_msCoreReferenceFinder);

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
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldsfld, fieldReference),
                Instruction.Create(OpCodes.Ldarg_1)
            });

            if (property.PropertyType.IsBoxingRequired(_catelType.SetValueInvoker.Parameters[1].ParameterType))
            {
                instructionsToAdd.Add(Instruction.Create(OpCodes.Box, property.PropertyType.Import()));
            }

            instructionsToAdd.Add(Instruction.Create(OpCodes.Call, _catelType.SetValueInvoker));
            instructionsToAdd.Add(Instruction.Create(OpCodes.Ret));

            var finalIndex = instructions.Insert(0, instructionsToAdd.ToArray());

            body.OptimizeMacros();

            return finalIndex;
        }

        private string GetBackingFieldName(PropertyDefinition property)
        {
            return $"<{property.Name}>k__BackingField";
        }

        private GenericInstanceType GetEventHandlerAdvancedPropertyChangedEventArgs(PropertyDefinition property)
        {
            var genericHandlerType = _msCoreReferenceFinder.GetCoreTypeReference("System.EventHandler`1");
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

            var declaringType = property.DeclaringType;

            var field = GetFieldDefinition(declaringType, fieldName, false);
            if (field != null)
            {
                foreach (var ctor in declaringType.GetConstructors())
                {
                    var ctorBody = ctor.Body;

                    ctorBody.SimplifyMacros();

                    var instructions = ctorBody.Instructions;
                    var validInstructionCounter = 0;

                    for (int i = 0; i < instructions.Count; i++)
                    {
                        var instruction = instructions[i];

                        if (instruction.IsOpCode(OpCodes.Nop))
                        {
                            continue;
                        }

                        validInstructionCounter++;

                        if (instruction.UsesField(field))
                        {
                            FodyEnvironment.LogDebug($"Field '{declaringType.FullName}.{field.Name}' is used in ctor '{ctor}'. Converting field usage to property usage to maintain compatibility with Catel generated properties.");

                            if (instruction.IsOpCode(OpCodes.Stfld))
                            {
                                // Setter
                                instruction.OpCode = OpCodes.Call;

                                // Note: make sure to support generic types
                                MethodReference setter = property.SetMethod;
                                var genericInstanceType = declaringType.MakeGenericIfRequired() as GenericInstanceType;
                                if (genericInstanceType != null)
                                {
                                    setter = setter.MakeHostInstanceGeneric(genericInstanceType.GenericArguments.ToArray());
                                }

                                instruction.Operand = declaringType.Module.ImportReference(setter);

                                // Now move this to the end of the method (we need to call the base ctor first to have the property bag ready)
                                instructions.MoveInstructionsToEnd(i - 2, 3);
                            }
                            else if (instruction.IsOpCode(OpCodes.Ldfld))
                            {
                                // Getter
                                instruction.OpCode = OpCodes.Call;

                                // Note: make sure to support generic types
                                MethodReference getter = property.GetMethod;
                                var genericInstanceType = declaringType.MakeGenericIfRequired() as GenericInstanceType;
                                if (genericInstanceType != null)
                                {
                                    getter = getter.MakeHostInstanceGeneric(genericInstanceType.GenericArguments.ToArray());
                                }

                                instruction.Operand = declaringType.Module.ImportReference(getter);

                                // Now move this to the end of the method (we need to call the base ctor first to have the property bag ready)
                                instructions.MoveInstructionsToEnd(i - 2, 3);

                            }
                            else if (instruction.IsOpCode(OpCodes.Ldflda))
                            {
                                // Probably setting a generic field value to a value by directly using an address. Since this was code like this:
                                //
                                // call instance !0 MyCompany.Models.Base.ItemsModel`1 < !T >::get_SelectedItem()
                                // initobj !T
                                //
                                // We need to generate code like this:
                                //
                                // ldloca.s local
                                // initobj !T
                                // ldloc.0
                                // call instance void Catel.Fody.TestAssembly.CSharp6_AutoPropertyInitializer_Generic_ExpectedCode`1 < !T >::set_SelectedItem(!0)

                                // Note: make sure to support generic types
                                MethodReference setter = property.SetMethod;
                                var genericInstanceType = declaringType.MakeGenericIfRequired() as GenericInstanceType;
                                if (genericInstanceType != null)
                                {
                                    setter = setter.MakeHostInstanceGeneric(genericInstanceType.GenericArguments.ToArray());
                                }

                                var variable = new VariableDefinition(property.PropertyType.MakeGenericIfRequired());
                                ctorBody.Variables.Add(variable);
                                ctorBody.InitLocals = true;

                                var newInstructions = new List<Instruction>();
                                newInstructions.Add(Instruction.Create(OpCodes.Ldloca, variable));
                                newInstructions.Add(instructions[i + 1]); // Just copy this instruction
                                newInstructions.Add(Instruction.Create(OpCodes.Ldloc, variable));
                                newInstructions.Add(Instruction.Create(OpCodes.Call, setter));

                                // Remove 2 instructions
                                instructions.RemoveAt(i);
                                instructions.RemoveAt(i);

                                // Now move this to the end of the method (we need to call the base ctor first to have the property bag ready)
                                instructions.Insert(instructions.Count - 1, newInstructions);
                            }
                            else
                            {
                                FodyEnvironment.LogError($"Field '{declaringType.FullName}.{field.Name}' is used in ctor '{ctor}'. Tried to convert it to property usage, but OpCode '{instruction.OpCode}' is not supported. Please raise an issue.");
                            }
                        }
                    }

                    ctorBody.OptimizeMacros();
                }

                declaringType.Fields.Remove(field);
            }
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