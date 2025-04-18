namespace Catel.Fody
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

#if DEBUG
    using System.Diagnostics;
#endif

    public class ModelBasePropertyWeaver : PropertyWeaverBase
    {
        private readonly Configuration _configuration;

        public ModelBasePropertyWeaver(CatelType catelType, CatelTypeProperty propertyData, Configuration configuration,
            ModuleWeaver moduleWeaver, MsCoreReferenceFinder msCoreReferenceFinder)
            : base(catelType, propertyData, moduleWeaver, msCoreReferenceFinder)
        {
            _configuration = configuration;
        }

        public void Execute(bool force = false)
        {
            var preferredSetValueInvoker = _catelType.PreferredSetValueInvoker;

            if (_catelType.GetValueInvoker is null || preferredSetValueInvoker is null)
            {
                return;
            }

            var property = _propertyData.PropertyDefinition;
            if (property is null)
            {
                FodyEnvironment.WriteWarning($"\t{_propertyData.Name} Skipping an unknown property because it has no property definition");
                return;
            }

            if (AlreadyContainsCallToMember(property.GetMethod, _catelType.GetValueInvoker.Name) ||
                AlreadyContainsCallToMember(property.SetMethod, preferredSetValueInvoker.Name))
            {
                FodyEnvironment.WriteDebug($"\t{property.GetName()} already has GetValue and/or SetValue functionality. Property will be ignored.");
                return;
            }

            if (!force && !HasBackingField(property))
            {
                FodyEnvironment.WriteDebug($"\t\tSkipping '{property.GetName()}' because it has no backing field");
                return;
            }

            if (ImplementsICommand(property))
            {
                FodyEnvironment.WriteDebug($"\t\tSkipping '{property.GetName()}' because it implements ICommand");
                return;
            }

            FodyEnvironment.WriteDebug("\t\t" + property.GetName());

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
                FodyEnvironment.WriteError($"\t\tFailed to handle property '{property.DeclaringType.Name}.{property.Name}'\n{ex.Message}\n{ex.StackTrace}");

#if DEBUG
                Debugger.Launch();
#endif
            }
        }

        private string GetChangeNotificationHandlerConstructorName(PropertyDefinition property)
        {
            var key = $"{property.DeclaringType.FullName}|{property.Name}";
            if (_cachedFieldInitializerNames.ContainsKey(key))
            {
                return _cachedFieldInitializerNames[key];
            }

            var counter = 0; // start at 0
            while (true)
            {
                var methodName = $"<.cctor>b__{counter}";
                if (GetMethodReference(property.DeclaringType, methodName, false) is null)
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
            if (staticConstructor is null)
            {
                FodyEnvironment.WriteDebug($"\t\t\t{type.Name} - adding static constructor");

                var voidType = _msCoreReferenceFinder.GetCoreTypeReference("Void");

                staticConstructor = new MethodDefinition(".cctor", MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static | MethodAttributes.RTSpecialName,
                    type.Module.ImportReference(voidType));

                var body = staticConstructor.Body;
                body.SimplifyMacros();

                body.Instructions.Add(Instruction.Create(OpCodes.Ret));

                body.OptimizeMacros();

                type.Methods.Add(staticConstructor);

                FodyEnvironment.WriteDebug($"\t\t\t{type.Name} - added static constructor");
            }
        }

        private void AddChangeNotificationHandlerField(PropertyDefinition property, CatelTypeProperty propertyData)
        {
            if (propertyData.ChangeCallbackReference is null)
            {
                return;
            }

            FodyEnvironment.WriteDebug($"\t\t\t{property.Name} - adding On{property.Name}Changed invocation");

            switch (_catelType.Version)
            {
                case CatelVersion.v5:
                    AddChangeNotificationHandlerField_Catel5(property, propertyData);
                    break;

                case CatelVersion.v6:
                    AddChangeNotificationHandlerField_Catel6(property, propertyData);
                    break;

                default:
                    throw new NotSupportedException($"Catel version '{_catelType.Version}' is not supported");
            }
        }

        private void AddChangeNotificationHandlerField_Catel5(PropertyDefinition property, CatelTypeProperty propertyData)
        {
            var declaringType = property.DeclaringType;
            var fieldName = GetChangeNotificationHandlerFieldName(property);

            var handlerType = GetPropertyChangedEventHandler_Catel4_Catel5(property);
            var eventArgsType = handlerType.GenericArguments[0];

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

            var initializationMethodName = GetChangeNotificationHandlerConstructorName(property);
            var initializationMethod = new MethodDefinition(initializationMethodName,
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static, declaringType.Module.ImportReference(voidType));

            initializationMethod.Parameters.Add(new ParameterDefinition("s", ParameterAttributes.None, declaringType.Module.ImportReference(objectType)));
            initializationMethod.Parameters.Add(new ParameterDefinition("e", ParameterAttributes.None, declaringType.Module.ImportReference(eventArgsType)));

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

        private void AddChangeNotificationHandlerField_Catel6(PropertyDefinition property, CatelTypeProperty propertyData)
        {
            var declaringType = property.DeclaringType;
            var fieldName = GetChangeNotificationHandlerFieldName(property);

            var handlerType = _msCoreReferenceFinder.GetCoreTypeReference("System.ComponentModel.PropertyChangedEventHandler");

            //.field private static class [System.ComponentModel]System.ComponentModel.PropertyChangedEventHandler CS$<>9__CachedAnonymousMethodDelegate1

            var field = new FieldDefinition(fieldName, FieldAttributes.Private | FieldAttributes.Static, declaringType.Module.ImportReference(handlerType));

            declaringType.Fields.Add(field);

            field.MarkAsCompilerGenerated(_msCoreReferenceFinder);

            //.method private hidebysig static void <.cctor>b__0(object s, class [System.ComponentModel]System.ComponentModel.PropertyChangedEventArgs e) cil managed
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

            var initializationMethodName = GetChangeNotificationHandlerConstructorName(property);
            var initializationMethod = new MethodDefinition(initializationMethodName,
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static, declaringType.Module.ImportReference(voidType));

            initializationMethod.Parameters.Add(new ParameterDefinition("s", ParameterAttributes.None, declaringType.Module.ImportReference(objectType)));
            initializationMethod.Parameters.Add(new ParameterDefinition("e", ParameterAttributes.None, _catelType.PropertyChangedEventArgsType));

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
            var fieldName = $"{property.Name}Property";
            var declaringType = property.DeclaringType;

            var attributes = FieldAttributes.Static | FieldAttributes.InitOnly;

            switch (_configuration.GeneratedPropertyDataAccessibility)
            {
                case Accessibility.Public:
                    attributes |= FieldAttributes.Public;
                    break;

                case Accessibility.Internal:
                    attributes |= FieldAttributes.Family;
                    break;

                case Accessibility.Private:
                    attributes |= FieldAttributes.Private;
                    break;
            }

            var fieldDefinition = new FieldDefinition(fieldName, attributes, _catelType.PropertyDataType);

            declaringType.Fields.Add(fieldDefinition);

            fieldDefinition.MarkAsCompilerGenerated(_msCoreReferenceFinder);

            return fieldDefinition;
        }

        private bool AddPropertyRegistration(PropertyDefinition property, CatelTypeProperty propertyData)
        {
            var fieldName = $"{property.Name}Property";
            var declaringType = property.DeclaringType;
            var fieldReference = GetFieldReference(declaringType, fieldName, true);
            if (fieldReference is null)
            {
                FodyEnvironment.WriteWarning($"\t\tCannot handle property '{_catelType.Name}.{property.Name}' because backing field is not found");
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
            if (exceptionHandler is not null)
            {
                instructionToInsert = exceptionHandler.TryStart;
            }

            var index = (instructionToInsert is not null) ? instructions.IndexOf(instructionToInsert) : instructions.Count;

            var instructionsToInsert = new List<Instruction>();

            switch (_catelType.Version)
            {
                case CatelVersion.v5:
                    instructionsToInsert.AddRange(CreatePropertyRegistration_Catel5(property, propertyData));
                    break;

                case CatelVersion.v6:
                    instructionsToInsert.AddRange(CreatePropertyRegistration_Catel6(property, propertyData));
                    break;

                default:
                    throw new NotSupportedException($"Catel version '{_catelType.Version}' is not supported");
            }

            var registerPropertyInvoker = (propertyData.DefaultValue is null) ? _catelType.RegisterPropertyWithoutDefaultValueInvoker : _catelType.RegisterPropertyWithDefaultValueInvoker;

            // Fill up the final booleans:
            // Catel v5: RegisterProperty([0] string name, 
            //                            [1] Type type,
            //                            [2] Func<object> createDefaultValue = null,
            //                            [3] EventHandler<AdvancedPropertyChangedEventArgs> propertyChangedEventHandler = null,
            //                            [4] bool includeInSerialization = true,
            //                            [5] bool includeInBackup = true)
            //
            // Catel v6: RegisterProperty<TValue>([0] string name, 
            //                                    [1] Func<TValue> createDefaultValue = null,
            //                                    [2] EventHandler<APropertyChangedEventArgs> propertyChangedEventHandler = null,
            //                                    [3] bool includeInSerialization = true,
            //                                    [4] bool includeInBackup = true)

            var parameters = registerPropertyInvoker.Parameters.Skip(3).ToList();
            var boolCount = 0;

            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                if (string.CompareOrdinal(parameter.ParameterType.FullName, _moduleWeaver.TypeSystem.BooleanDefinition.FullName) != 0)
                {
                    continue;
                }

                boolCount++;

                // includeInBackup == 2nd bool value
                if (!propertyData.IncludeInBackup && boolCount == 2)
                {
                    instructionsToInsert.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                }
                else
                {
                    instructionsToInsert.Add(Instruction.Create(OpCodes.Ldc_I4_1));
                }
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

                    genericRegisterProperty.GenericArguments.Add(property.PropertyType.Import(false));
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

        private List<Instruction> CreatePropertyRegistration_Catel5(PropertyDefinition property, CatelTypeProperty propertyData)
        {
            // Catel v5:
            //
            //L_0000: ldstr "FullName"
            //L_0005: ldtoken string // note that this is the property type
            //L_000a: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
            //L_000f: ldnull
            //L_0010: ldnull
            //L_0011: ldc.i4.1
            //L_0012: call class [Catel.Core]Catel.Data.PropertyData [Catel.Core]Catel.Data.ModelBase::RegisterProperty(string, class [mscorlib]System.Type, class [mscorlib]System.Func`1<object>, class [mscorlib]System.EventHandler`1<class [Catel.Core]Catel.Data.AdvancedPropertyChangedEventArgs>, bool)
            //L_0017: stsfld class [Catel.Core]Catel.Data.PropertyData Catel.Fody.TestAssembly.ViewModelBaseTest::FullNameProperty

            var instructions = new List<Instruction>();
            var getTypeFromHandle = property.Module.GetMethodAndImport("GetTypeFromHandle");

            instructions.AddRange(new[]
            {
                Instruction.Create(OpCodes.Ldstr, property.Name),
                Instruction.Create(OpCodes.Ldtoken, property.PropertyType.Import()),
                Instruction.Create(OpCodes.Call, getTypeFromHandle),
            });

            instructions.AddRange(CreateDefaultValueInstructions(propertyData));

            if (propertyData.ChangeCallbackReference is not null)
            {
                instructions.AddRange(CreateChangeCallbackReference_Catel5(property));
            }
            else
            {
                instructions.Add(Instruction.Create(OpCodes.Ldnull));
            }

            return instructions;
        }

        private List<Instruction> CreatePropertyRegistration_Catel6(PropertyDefinition property, CatelTypeProperty propertyData)
        {
            // Catel v6:
            //
            //L_0000: ldstr "FullName"
            //L_0005: ldtoken string // note that this is the property type
            //L_000a: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
            //L_000f: ldnull
            //L_0010: ldnull
            //L_0011: ldc.i4.1
            //L_0012: call class [Catel.Core]Catel.Data.IPropertyData [Catel.Core]Catel.Data.ModelBase::RegisterProperty<TValue>(string, class [mscorlib]System.Func`1<TValue>, class [mscorlib]System.EventHandler`1<class [Catel.Core]Catel.Data.PropertyChangedEventArgs>, bool)
            //L_0017: stsfld class [Catel.Core]Catel.Data.IPropertyData Catel.Fody.TestAssembly.ViewModelBaseTest::FullNameProperty

            var instructions = new List<Instruction>();

            instructions.AddRange(new[]
            {
                Instruction.Create(OpCodes.Ldstr, property.Name),
            });

            instructions.AddRange(CreateDefaultValueInstructions(propertyData));

            if (propertyData.ChangeCallbackReference is not null)
            {
                instructions.AddRange(CreateChangeCallbackReference_Catel6(property));
            }
            else
            {
                instructions.Add(Instruction.Create(OpCodes.Ldnull));
            }

            return instructions;
        }

        private List<Instruction> CreateDefaultValueInstructions(CatelTypeProperty propertyData)
        {
            var instructions = new List<Instruction>();

            var resolvedPropertyType = propertyData.PropertyDefinition.PropertyType.Resolve();

            // Default value
            if (propertyData.DefaultValue is string stringValue)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldstr, stringValue));
            }
            else if (propertyData.DefaultValue is bool boolValue)
            {
                if (boolValue)
                {
                    instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
                }
                else
                {
                    instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                }
            }
            else if (propertyData.DefaultValue is int intValue)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, intValue));
            }
            else if (propertyData.DefaultValue is long longValue)
            {
                if (longValue <= int.MaxValue)
                {
                    // Note: don't use Ldc_I8 here, although it is a long
                    instructions.Add(Instruction.Create(OpCodes.Ldc_I4, (int)longValue));
                    instructions.Add(Instruction.Create(OpCodes.Conv_I8));
                }
                else
                {
                    instructions.Add(Instruction.Create(OpCodes.Ldc_I8, longValue));
                }
            }
            else if (propertyData.DefaultValue is float floatValue)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldc_R4, floatValue));
            }
            else if (propertyData.DefaultValue is double doubleValue)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldc_R8, doubleValue));
            }
            else if (resolvedPropertyType is not null && resolvedPropertyType.IsEnum && propertyData.DefaultValue is not null)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, (int)((CustomAttributeArgument)propertyData.DefaultValue).Value));
            }
            else
            {
                instructions.Add(Instruction.Create(OpCodes.Ldnull));
            }

            // Special case: if Nullable<ValueType>, we need to do special things
            if (propertyData.PropertyDefinition.PropertyType.IsNullableValueType() &&
                propertyData.DefaultValue is null == false)
            {
                // IL_0033: ldc.i4.1 // already done above)
                // IL_0034: newobj instance void valuetype [System.Runtime]System.Nullable`1<bool>::.ctor(!0)

                var declaringType = (GenericInstanceType)propertyData.PropertyDefinition.PropertyType;
                var resolvedType = declaringType.Resolve();

                resolvedType.FixPrivateCorLibScope();

                var ctorDefinition = resolvedType.Constructor(false);
                var importedCtor = _catelType.TypeDefinition.Module.ImportReference(ctorDefinition);
                var ctor = importedCtor.MakeHostInstanceGeneric(declaringType.GenericArguments[0]);

                instructions.Add(Instruction.Create(OpCodes.Newobj, ctor));
            }

            return instructions;
        }

        private List<Instruction> CreateChangeCallbackReference_Catel5(PropertyDefinition property)
        {
            var instructions = new List<Instruction>();

            var declaringType = property.DeclaringType;

            //L_0040: ldsfld class [mscorlib]System.EventHandler`1<class [Catel.Core]Catel.Data.AdvancedPropertyChangedEventArgs> Catel.Fody.TestAssembly.ModelBaseTest::CS$<>9__CachedAnonymousMethodDelegate1
            //L_0045: brtrue.s L_005a
            //L_0047: ldnull
            //L_0048: ldftn void Catel.Fody.TestAssembly.ModelBaseTest::<.cctor>b__0(object, class [Catel.Core]Catel.Data.AdvancedPropertyChangedEventArgs)
            //L_004e: newobj instance void [mscorlib]System.EventHandler`1<class [Catel.Core]Catel.Data.AdvancedPropertyChangedEventArgs>::.ctor(object, native int)
            //L_0053: stsfld class [mscorlib]System.EventHandler`1<class [Catel.Core]Catel.Data.AdvancedPropertyChangedEventArgs> Catel.Fody.TestAssembly.ModelBaseTest::CS$<>9__CachedAnonymousMethodDelegate1
            //L_0058: br.s L_005a
            //L_005a: ldsfld class [mscorlib]System.EventHandler`1<class [Catel.Core]Catel.Data.AdvancedPropertyChangedEventArgs> Catel.Fody.TestAssembly.ModelBaseTest::CS$<>9__CachedAnonymousMethodDelegate1

            var handlerFieldName = GetChangeNotificationHandlerFieldName(property);
            var handlerConstructorFieldName = GetChangeNotificationHandlerConstructorName(property);

            var handlerField = GetFieldReference(property.DeclaringType, handlerFieldName, true);
            var handlerConstructor = GetMethodReference(property.DeclaringType, handlerConstructorFieldName, true);
            var handlerType = GetPropertyChangedEventHandler_Catel4_Catel5(property);
            var importedHandlerType = handlerType.Resolve();

            var advancedPropertyChangedEventArgsType = property.Module.FindType("Catel.Core", "Catel.Data.AdvancedPropertyChangedEventArgs");
            var handlerTypeConstructor = declaringType.Module.ImportReference(importedHandlerType.Constructor(false));
            var genericConstructor = handlerTypeConstructor.MakeHostInstanceGeneric(declaringType.Module.ImportReference(advancedPropertyChangedEventArgsType));

            var finalInstruction = Instruction.Create(OpCodes.Ldsfld, handlerField);

            instructions.AddRange(new[]
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

            return instructions;
        }

        private List<Instruction> CreateChangeCallbackReference_Catel6(PropertyDefinition property)
        {
            var instructions = new List<Instruction>();

            var declaringType = property.DeclaringType;

            //L_0040: ldsfld class [System.ComponentModel]System.ComponentModel.PropertyChangedEventHandler Catel.Fody.TestAssembly.ModelBaseTest::CS$<>9__CachedAnonymousMethodDelegate1
            //L_0045: brtrue.s L_005a
            //L_0047: ldnull
            //L_0048: ldftn void Catel.Fody.TestAssembly.ModelBaseTest::<.cctor>b__0(object, class [System.ComponentModel]System.ComponentModel.PropertyChangedEventArgs)
            //L_004e: newobj instance void [System.ComponentModel]System.ComponentModel.PropertyChangedEventHandler::.ctor(object, native int)
            //L_0053: stsfld class [System.ComponentModel]System.ComponentModel.PropertyChangedEventHandler Catel.Fody.TestAssembly.ModelBaseTest::CS$<>9__CachedAnonymousMethodDelegate1
            //L_0058: br.s L_005a
            //L_005a: ldsfld class [System.ComponentModel]System.ComponentModel.PropertyChangedEventHandler Catel.Fody.TestAssembly.ModelBaseTest::CS$<>9__CachedAnonymousMethodDelegate1

            var handlerFieldName = GetChangeNotificationHandlerFieldName(property);
            var handlerConstructorFieldName = GetChangeNotificationHandlerConstructorName(property);

            var handlerField = GetFieldReference(property.DeclaringType, handlerFieldName, true);
            var handlerConstructor = GetMethodReference(property.DeclaringType, handlerConstructorFieldName, true);
            var handlerType = _msCoreReferenceFinder.GetCoreTypeReference("System.ComponentModel.PropertyChangedEventHandler");
            var importedHandlerType = handlerType.Resolve();

            var handlerTypeConstructor = declaringType.Module.ImportReference(importedHandlerType.Constructor(false));

            var finalInstruction = Instruction.Create(OpCodes.Ldsfld, handlerField);

            instructions.AddRange(new[]
            {
                Instruction.Create(OpCodes.Ldsfld, handlerField),
                Instruction.Create(OpCodes.Brtrue_S, finalInstruction),
                Instruction.Create(OpCodes.Ldnull),
                Instruction.Create(OpCodes.Ldftn, handlerConstructor),
                Instruction.Create(OpCodes.Newobj, handlerTypeConstructor),
                Instruction.Create(OpCodes.Stsfld, handlerField),
                Instruction.Create(OpCodes.Br_S, finalInstruction),
                finalInstruction
            });

            return instructions;
        }

        private int AddGetValueCall(PropertyDefinition property, FieldReference fieldReference)
        {
            FodyEnvironment.WriteDebug($"\t\t\t{property.Name} - adding GetValue call");

            var genericGetValue = new GenericInstanceMethod(_catelType.GetValueInvoker);

            foreach (var genericParameter in _catelType.GetValueInvoker.GenericParameters)
            {
                genericGetValue.GenericParameters.Add(genericParameter);
            }

            genericGetValue.GenericArguments.Add(property.PropertyType.Import());

            if (property.GetMethod is null)
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
            FodyEnvironment.WriteDebug($"\t\t\t{property.Name} - adding SetValue call");

            //string fieldName = string.Format("{0}Property", property.Name);
            //var declaringType = property.DeclaringType;
            //var fieldReference = GetField(declaringType, fieldName);

            // Catel v5: SetValue<TValue>(PropertyData propertyName, TValue value)
            // Catel v6: SetValue<TValue>(IPropertyData propertyName, TValue value)

            if (property.SetMethod is null)
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

            // Check if Catel 5.12+ SetValue<TValue> is available
            var preferredSetValueInvoker = _catelType.PreferredSetValueInvoker;
            if (preferredSetValueInvoker.HasGenericParameters)
            {
                // Generic, no boxing required, but we need to make it generic
                var genericSetValue = new GenericInstanceMethod(preferredSetValueInvoker);

                foreach (var genericParameter in preferredSetValueInvoker.GenericParameters)
                {
                    genericSetValue.GenericParameters.Add(genericParameter);
                }

                genericSetValue.GenericArguments.Add(property.PropertyType.Import());

                preferredSetValueInvoker = genericSetValue;
            }
            else
            {
                // Non-generic, requires boxing
                if (property.PropertyType.IsBoxingRequired(_catelType.SetValueInvoker.Parameters[1].ParameterType))
                {
                    instructionsToAdd.Add(Instruction.Create(OpCodes.Box, property.PropertyType.Import()));
                }
            }

            if (preferredSetValueInvoker.Parameters.Count > 2)
            {
                // Catel v5 is a new signature:
                // protected internal void SetValue(string name, object value, bool notifyOnChange = true)
                instructionsToAdd.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            }

            instructionsToAdd.Add(Instruction.Create(OpCodes.Call, preferredSetValueInvoker));
            instructionsToAdd.Add(Instruction.Create(OpCodes.Ret));

            var finalIndex = instructions.Insert(0, instructionsToAdd.ToArray());

            body.OptimizeMacros();

            return finalIndex;
        }

        private bool ImplementsICommand(PropertyDefinition property)
        {
            var commandInterfaces = new List<string>(new[] { "ICommand", "ICatelCommand" });

            var resolvedType = property.PropertyType.Resolve();
            if (resolvedType is null)
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
            if (field is not null)
            {
                foreach (var ctor in declaringType.GetConstructors())
                {
                    var ctorBody = ctor.Body;

                    ctorBody.SimplifyMacros();

                    var instructions = ctorBody.Instructions;
                    var validInstructionCounter = 0;

                    for (var i = 0; i < instructions.Count; i++)
                    {
                        var instruction = instructions[i];
                        if (instruction.IsOpCode(OpCodes.Nop))
                        {
                            continue;
                        }

                        validInstructionCounter++;

                        if (instruction.UsesField(field))
                        {
                            FodyEnvironment.WriteDebug($"Field '{declaringType.FullName}.{field.Name}' is used in ctor '{ctor}'. Converting field usage to property usage to maintain compatibility with Catel generated properties.");

                            if (instruction.IsOpCode(OpCodes.Stfld))
                            {
                                // Replace
                                // 
	                            // IL_0014: ldarg.0
	                            // IL_0015: ldarg.1
	                            // IL_0016: stfld class Catel.Fody.TestAssembly.Bugs.GH0473.TestModel Catel.Fody.TestAssembly.Bugs.GH0473.GH0473ViewModel::'<Model>k__BackingField' /* 04000097 */
                                // 
                                // with
                                //
	                            // IL_0014: ldarg.0
	                            // IL_0015: ldarg.1
	                            // IL_0016: call instance void Catel.Fody.TestAssembly.Bugs.GH0473.GH0473ViewModel_Expected::set_Model(class Catel.Fody.TestAssembly.Bugs.GH0473.TestModel) /* 06000205 */

                                // Setter
                                instruction.OpCode = OpCodes.Call;

                                // Note: make sure to support generic types
                                MethodReference setter = property.SetMethod;
                                if (declaringType.MakeGenericIfRequired() is GenericInstanceType genericInstanceType)
                                {
                                    setter = setter.MakeHostInstanceGeneric(genericInstanceType.GenericArguments.ToArray());
                                }

                                instruction.Operand = declaringType.Module.ImportReference(setter);

                                // Now move this to the end of the method (we need to call the base ctor first to have the property bag ready)
                                var baseIndex = ctor.FindBaseConstructorIndex();
                                if (baseIndex > i)
                                {
                                    // After a call to a ctor, a double nop is required
                                    var indexToInsert = baseIndex + 1;
                                    if (instructions.IsNextInstructionOpCode(baseIndex, OpCodes.Nop))
                                    {
                                        indexToInsert++;

                                        if (instructions.IsNextInstructionOpCode(baseIndex + 1, OpCodes.Nop))
                                        {
                                            indexToInsert++;
                                        }
                                    }

                                    instructions.MoveInstructionsToPosition(i - 2, 3, indexToInsert);
                                }
                            }
                            else if (instruction.IsOpCode(OpCodes.Ldfld))
                            {
                                // Getter
                                instruction.OpCode = OpCodes.Call;

                                // Note: make sure to support generic types
                                MethodReference getter = property.GetMethod;
                                if (declaringType.MakeGenericIfRequired() is GenericInstanceType genericInstanceType)
                                {
                                    getter = getter.MakeHostInstanceGeneric(genericInstanceType.GenericArguments.ToArray());
                                }

                                instruction.Operand = declaringType.Module.ImportReference(getter);

                                // Now move this to the end of the method (we need to call the base ctor first to have the property bag ready)
                                var baseIndex = ctor.FindBaseConstructorIndex();
                                if (baseIndex > i)
                                {
                                    // After a call to a ctor, a double nop is required
                                    var indexToInsert = baseIndex + 1;
                                    if (instructions.IsNextInstructionOpCode(baseIndex, OpCodes.Nop))
                                    {
                                        indexToInsert++;

                                        if (instructions.IsNextInstructionOpCode(baseIndex + 1, OpCodes.Nop))
                                        {
                                            indexToInsert++;
                                        }
                                    }

                                    instructions.MoveInstructionsToPosition(i - 2, 3, indexToInsert);
                                }
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
                                if (declaringType.MakeGenericIfRequired() is GenericInstanceType genericInstanceType)
                                {
                                    setter = setter.MakeHostInstanceGeneric(genericInstanceType.GenericArguments.ToArray());
                                }

                                var variable = new VariableDefinition(property.PropertyType.MakeGenericIfRequired());
                                ctorBody.Variables.Add(variable);
                                ctorBody.InitLocals = true;

                                var newInstructions = new List<Instruction>
                                {
                                    Instruction.Create(OpCodes.Ldloca, variable),
                                    instructions[i + 1], // Just copy this initobj !T instruction
                                    Instruction.Create(OpCodes.Ldloc, variable),
                                    Instruction.Create(OpCodes.Call, setter)
                                };

                                // Remove 3 instructions
                                // ldarg
                                // ldflda
                                // init T
                                instructions.RemoveAt(i);
                                instructions.RemoveAt(i);

                                if (instructions[i - 1].IsOpCode(OpCodes.Ldarg, OpCodes.Ldarg_0))
                                {
                                    newInstructions.Insert(0, Instruction.Create(OpCodes.Ldarg_0));
                                    instructions.RemoveAt(i - 1);
                                }

                                var indexToInsert = i + 1;

                                // Now move this to the end of the method (we need to call the base ctor first to have the property bag ready)
                                var baseIndex = ctor.FindBaseConstructorIndex();
                                if (baseIndex < 0)
                                {
                                    FodyEnvironment.WriteError($"Field '{declaringType.FullName}.{field.Name}' is used in ctor '{ctor}'. A rare condition occurred (no base ctor found), please contact support");
                                    continue;
                                }

                                if (baseIndex > i)
                                {
                                    // After a call to a ctor, a double nop is required
                                    indexToInsert = baseIndex + 1;
                                    if (instructions.IsNextInstructionOpCode(baseIndex, OpCodes.Nop))
                                    {
                                        indexToInsert++;

                                        if (instructions.IsNextInstructionOpCode(baseIndex + 1, OpCodes.Nop))
                                        {
                                            indexToInsert++;
                                        }
                                    }
                                }

                                instructions.Insert(indexToInsert, newInstructions);
                            }
                            else
                            {
                                FodyEnvironment.WriteError($"Field '{declaringType.FullName}.{field.Name}' is used in ctor '{ctor}'. Tried to convert it to property usage, but OpCode '{instruction.OpCode}' is not supported. Please raise an issue.");
                            }
                        }
                    }

                    ctorBody.OptimizeMacros();
                    ctor.UpdateDebugInfo();
                }

                declaringType.Fields.Remove(field);
            }
        }

        public static bool AlreadyContainsCallToMember(MethodDefinition methodDefinition, string methodName)
        {
            if (methodDefinition is null)
            {
                return false;
            }

            var instructions = methodDefinition.Body.Instructions;
            return instructions.Any(x => x.OpCode.IsCall() &&
                                         x.Operand is MethodReference reference &&
                                         reference.Name == methodName);
        }
    }
}
