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

    public class ObservableObjectPropertyWeaver : PropertyWeaverBase
    {
        private readonly MethodDefinition _equalityOperationMethod;

        public ObservableObjectPropertyWeaver(CatelType catelType, CatelTypeProperty propertyData, ModuleWeaver moduleWeaver,
            MsCoreReferenceFinder msCoreReferenceFinder)
            : base(catelType, propertyData, moduleWeaver, msCoreReferenceFinder)
        {
            var stringType = (TypeDefinition)msCoreReferenceFinder.GetCoreTypeReference("System.String");

            _equalityOperationMethod = stringType.Resolve().Methods.First(x => x.Name == "op_Equality");
        }

        public void Execute(bool force = false)
        {
            var property = _propertyData.PropertyDefinition;
            if (property is null)
            {
                FodyEnvironment.WriteWarning("Skipping an unknown property because it has no property definition");
                return;
            }

            if (!force && !HasBackingField(property))
            {
                FodyEnvironment.WriteDebug($"\t\tSkipping '{property.GetName()}' because it has no backing field");
                return;
            }

            if (!IsCleanSetter(property))
            {
                FodyEnvironment.WriteDebug($"\t\tSkipping '{property.GetName()}' because it has no clean setter (custom implementation?)");
                return;
            }

            FodyEnvironment.WriteDebug("\t\t" + property.Name);

            try
            {
                UpdateSetValueCall(property);
            }
            catch (Exception ex)
            {
                FodyEnvironment.WriteError($"\t\tFailed to handle property '{property.DeclaringType.Name}.{property.Name}'\n{ex.Message}\n{ex.StackTrace}");

#if DEBUG
                Debugger.Launch();
#endif
            }
        }

        private bool IsCleanSetter(PropertyDefinition property)
        {
            // A clean setter looks like this:
            // IL_0000: ldarg.0      // this
            // IL_0001: ldarg.1      // 'value'
            // IL_0002: stfld        bool Catel.Fody.TestAssembly.ObservableObjectTest_Expected/*0200004A*/::_onLastNameChangedCallbackCalled/*040000B7*/
            // IL_0007: ret

            var setter = property.SetMethod;
            if (setter is null)
            {
                return false;
            }

            var instructions = setter.Body.Instructions;

            for (var i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];

                if (instruction.IsOpCode(OpCodes.Nop, OpCodes.Ret) ||
                    instruction.IsOpCode(OpCodes.Ldarg, OpCodes.Ldarg_0, OpCodes.Ldarg_1) ||
                    instruction.IsOpCode(OpCodes.Stfld))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private void UpdateSetValueCall(PropertyDefinition property)
        {
            var setMethod = property.SetMethod;
            var body = setMethod.Body;
            body.SimplifyMacros();

            var instructions = body.Instructions;

            // We want to change this:
            //
            // set
            // {
            //     _myField = value;
            // }
            //
            // to
            // set
            // {
            //     // DEPENDING ON TYPE, WE USE DIFFERENT CHECK
            //     if (_valueType == value)
            //     {
            //          return;
            //     }
            //
            //     if (_stringType == value)
            //     {
            //          return;
            //     }
            //
            //     if (ReferenceEquals(_referenceType, value))
            //     {
            //          return;
            //     }
            //
            //     _myField = value;
            //     OnMyFieldChanged();                     // Step 2, optional!
            //     RaisePropertyChanged(nameof(MyField));  // Step 3
            // }


            // Note: we cannot support this on generic types where the value
            // is generic (e.g. T? Property)
            var isGeneric = property.PropertyType.IsGenericParameter;

            var setFieldInstruction = instructions.FirstOrDefault(x => x.OpCode == OpCodes.Stfld);
            var fieldReference = setFieldInstruction?.Operand as FieldReference;
            if (!isGeneric && fieldReference is not null)
            {
                var boolVariable = new VariableDefinition(_moduleWeaver.ModuleDefinition.ImportReference(_msCoreReferenceFinder.GetCoreTypeReference("Boolean")));
                setMethod.Body.Variables.Add(boolVariable);

                var propertyType = _propertyData.PropertyDefinition.PropertyType;
                if (propertyType.IsValueType)
                {
                    // Nullables need to be treated differently
                    var isNullable = propertyType.FullName.StartsWith("System.Nullable`1");
                    if (isNullable)
                    {
                        // Extra local variables
                        var variable1 = new VariableDefinition(_moduleWeaver.ModuleDefinition.ImportReference(propertyType));
                        var variable2 = new VariableDefinition(_moduleWeaver.ModuleDefinition.ImportReference(propertyType));

                        setMethod.Body.Variables.Add(variable1);
                        setMethod.Body.Variables.Add(variable2);

                        var actualValueType = ((GenericInstanceType)propertyType).GenericArguments[0];

                        var hasValueGetter = _moduleWeaver.ModuleDefinition.ImportReference(propertyType.Resolve().Methods.Single(x => x.Name == "get_HasValue"))
                            .MakeHostInstanceGeneric(actualValueType);
                        var getValueOrDefaultMethod = _moduleWeaver.ModuleDefinition.ImportReference(propertyType.Resolve().Methods.Single(x => x.Name == "GetValueOrDefault" && !x.Parameters.Any()))
                            .MakeHostInstanceGeneric(actualValueType);

                        var equalityOperator = actualValueType.Resolve().Methods.FirstOrDefault(x => x.Name == "op_Equality" && x.IsStatic);
                        if (equalityOperator is not null)
                        {
                            //IL_0001: ldarg.1
                            //IL_0002: stloc.1
                            //IL_0003: ldarg.0
                            //IL_0004: ldfld valuetype [System.Runtime]System.Nullable`1<valuetype [System.Runtime]System.DateTimeOffset> Catel.Fody.TestAssembly.ObservableObjectTest_Expected::_nullableDateTimeOffsetProperty
                            //IL_0009: stloc.2
                            //IL_000a: ldloca.s 1
                            //IL_000c: call instance bool valuetype [System.Runtime]System.Nullable`1<valuetype [System.Runtime]System.DateTimeOffset>::get_HasValue()
                            //IL_0011: ldloca.s 2
                            //IL_0013: call instance bool valuetype [System.Runtime]System.Nullable`1<valuetype [System.Runtime]System.DateTimeOffset>::get_HasValue()
                            //IL_0018: beq.s IL_001d

                            //IL_001a: ldc.i4.0
                            //IL_001b: br.s IL_003c

                            //IL_001d: ldloca.s 1
                            //IL_001f: call instance bool valuetype [System.Runtime]System.Nullable`1<valuetype [System.Runtime]System.DateTimeOffset>::get_HasValue()
                            //IL_0024: brtrue.s IL_0029

                            //IL_0026: ldc.i4.1
                            //IL_0027: br.s IL_003c

                            //IL_0029: ldloca.s 1
                            //IL_002b: call instance !0 valuetype [System.Runtime]System.Nullable`1<valuetype [System.Runtime]System.DateTimeOffset>::GetValueOrDefault()
                            //IL_0030: ldloca.s 2
                            //IL_0032: call instance !0 valuetype [System.Runtime]System.Nullable`1<valuetype [System.Runtime]System.DateTimeOffset>::GetValueOrDefault()
                            //IL_0037: call bool [System.Runtime]System.DateTimeOffset::op_Equality(valuetype [System.Runtime]System.DateTimeOffset, valuetype [System.Runtime]System.DateTimeOffset)

                            //IL_003c: stloc.0
                            //IL_003d: ldloc.0
                            //IL_003e: brfalse.s IL_0043

                            var ldloca1Instruction1 = Instruction.Create(OpCodes.Ldloca_S, variable1);
                            var ldloca1Instruction2 = Instruction.Create(OpCodes.Ldloca_S, variable1);
                            var stloc0Instruction = Instruction.Create(OpCodes.Stloc_0);

                            var instructionsToInsert = new List<Instruction>(new[]
                            {
                                Instruction.Create(OpCodes.Ldarg_1),
                                Instruction.Create(OpCodes.Stloc_1),
                                Instruction.Create(OpCodes.Ldarg_0),
                                Instruction.Create(OpCodes.Ldfld, fieldReference),
                                Instruction.Create(OpCodes.Stloc_2),
                                Instruction.Create(OpCodes.Ldloca_S, variable1),
                                Instruction.Create(OpCodes.Call, hasValueGetter),
                                Instruction.Create(OpCodes.Ldloca_S, variable2),
                                Instruction.Create(OpCodes.Call, hasValueGetter),
                                Instruction.Create(OpCodes.Beq_S, ldloca1Instruction1),
                                Instruction.Create(OpCodes.Ldc_I4_0),
                                Instruction.Create(OpCodes.Br_S, stloc0Instruction),
                                ldloca1Instruction1,
                                Instruction.Create(OpCodes.Call, hasValueGetter),
                                Instruction.Create(OpCodes.Brtrue_S, ldloca1Instruction2),
                                Instruction.Create(OpCodes.Ldc_I4_1),
                                Instruction.Create(OpCodes.Br_S, stloc0Instruction),
                                ldloca1Instruction2,
                                Instruction.Create(OpCodes.Call, getValueOrDefaultMethod),
                                Instruction.Create(OpCodes.Ldloca_S, variable2),
                                Instruction.Create(OpCodes.Call, getValueOrDefaultMethod),
                                Instruction.Create(OpCodes.Call, _moduleWeaver.ModuleDefinition.ImportReference(equalityOperator)),
                                stloc0Instruction,
                                Instruction.Create(OpCodes.Ldloc_0),
                                Instruction.Create(OpCodes.Brfalse_S, instructions.First(x => !x.IsOpCode(OpCodes.Nop))),
                                Instruction.Create(OpCodes.Br_S, instructions.Last(x => x.IsOpCode(OpCodes.Ret)))
                            });

                            instructions.Insert(0, instructionsToInsert);
                        }
                        else
                        {
                            //IL_0001: ldarg.1
                            //IL_0002: stloc.1
                            //IL_0003: ldarg.0
                            //IL_0004: ldfld valuetype [System.Runtime]System.Nullable`1<bool> Catel.Fody.TestAssembly.ObservableObjectTest_Expected::_nullableBoolProperty
                            //IL_0009: stloc.2
                            //IL_000a: ldloca.s 1
                            //IL_000c: call instance !0 valuetype [System.Runtime]System.Nullable`1<bool>::GetValueOrDefault()
                            //IL_0011: ldloca.s 2
                            //IL_0013: call instance !0 valuetype [System.Runtime]System.Nullable`1<bool>::GetValueOrDefault()
                            //IL_0018: ceq
                            //IL_001a: ldloca.s 1
                            //IL_001c: call instance bool valuetype [System.Runtime]System.Nullable`1<bool>::get_HasValue()
                            //IL_0021: ldloca.s 2
                            //IL_0023: call instance bool valuetype [System.Runtime]System.Nullable`1<bool>::get_HasValue()
                            //IL_0028: ceq
                            //IL_002a: and
                            //IL_002b: stloc.0
                            //IL_002c: ldloc.0
                            //IL_002d: brfalse.s IL_0032

                            //IL_002f: nop
                            //IL_0030: br.s IL_0045

                            var ldloca1Instruction1 = Instruction.Create(OpCodes.Ldloca_S, variable1);
                            var ldloca1Instruction2 = Instruction.Create(OpCodes.Ldloca_S, variable1);
                            var stloc0Instruction = Instruction.Create(OpCodes.Stloc_0);

                            var instructionsToInsert = new List<Instruction>(new[]
                            {
                                Instruction.Create(OpCodes.Ldarg_1),
                                Instruction.Create(OpCodes.Stloc_1),
                                Instruction.Create(OpCodes.Ldarg_0),
                                Instruction.Create(OpCodes.Ldfld, fieldReference),
                                Instruction.Create(OpCodes.Stloc_2),
                                Instruction.Create(OpCodes.Ldloca_S, variable1),
                                Instruction.Create(OpCodes.Call, getValueOrDefaultMethod),
                                Instruction.Create(OpCodes.Ldloca_S, variable2),
                                Instruction.Create(OpCodes.Call, getValueOrDefaultMethod),
                                Instruction.Create(OpCodes.Ceq),
                                Instruction.Create(OpCodes.Ldloca_S, variable1),
                                Instruction.Create(OpCodes.Call, hasValueGetter),
                                Instruction.Create(OpCodes.Ldloca_S, variable2),
                                Instruction.Create(OpCodes.Call, hasValueGetter),
                                Instruction.Create(OpCodes.Ceq),
                                Instruction.Create(OpCodes.And),
                                Instruction.Create(OpCodes.Stloc_0),
                                Instruction.Create(OpCodes.Ldloc_0),
                                Instruction.Create(OpCodes.Brfalse_S, instructions.First(x => !x.IsOpCode(OpCodes.Nop))),
                                Instruction.Create(OpCodes.Br_S, instructions.Last(x => x.IsOpCode(OpCodes.Ret)))
                            });

                            instructions.Insert(0, instructionsToInsert);
                        }
                    }
                    else
                    {
                        // Non-nullable
                        var instructionsToInsert = new List<Instruction>(new[]
                        {
                            Instruction.Create(OpCodes.Ldarg_1),
                            Instruction.Create(OpCodes.Ldarg_0),
                            Instruction.Create(OpCodes.Ldfld, fieldReference)
                        });

                        var equalityOperator = _propertyData.PropertyDefinition.PropertyType.Resolve().Methods.FirstOrDefault(x => x.Name == "op_Equality" && x.IsStatic);
                        if (equalityOperator is not null)
                        {
                            // call op_Equality
                            instructionsToInsert.Add(Instruction.Create(OpCodes.Call, _moduleWeaver.ModuleDefinition.ImportReference(equalityOperator)));
                        }
                        else
                        {
                            // == (ceq)
                            instructionsToInsert.Add(Instruction.Create(OpCodes.Ceq));
                        }

                        instructionsToInsert.AddRange(new[]
                        {
                            Instruction.Create(OpCodes.Stloc_0),
                            Instruction.Create(OpCodes.Ldloc_0),
                            Instruction.Create(OpCodes.Brfalse_S, instructions.First(x => !x.IsOpCode(OpCodes.Nop))),
                            Instruction.Create(OpCodes.Br_S, instructions.Last(x => x.IsOpCode(OpCodes.Ret)))
                        });

                        instructions.Insert(0, instructionsToInsert);
                    }
                }
                else if (_propertyData.PropertyDefinition.PropertyType.FullName == "System.String")
                {
                    // ==
                    instructions.Insert(0,
                        Instruction.Create(OpCodes.Ldarg_1),
                        Instruction.Create(OpCodes.Ldarg_0),
                        Instruction.Create(OpCodes.Ldfld, fieldReference),
                        Instruction.Create(OpCodes.Call, _moduleWeaver.ModuleDefinition.ImportReference(_equalityOperationMethod)),
                        Instruction.Create(OpCodes.Stloc_0),
                        Instruction.Create(OpCodes.Ldloc_0),
                        Instruction.Create(OpCodes.Brfalse_S, instructions.First(x => !x.IsOpCode(OpCodes.Nop))),
                        Instruction.Create(OpCodes.Br_S, instructions.Last(x => x.IsOpCode(OpCodes.Ret))));
                }
                else
                {
                    // ReferenceEquals (is ceq)
                    instructions.Insert(0,
                        Instruction.Create(OpCodes.Ldarg_1),
                        Instruction.Create(OpCodes.Ldarg_0),
                        Instruction.Create(OpCodes.Ldfld, fieldReference),
                        Instruction.Create(OpCodes.Ceq),
                        Instruction.Create(OpCodes.Stloc_0),
                        Instruction.Create(OpCodes.Ldloc_0),
                        Instruction.Create(OpCodes.Brfalse_S, instructions.First(x => !x.IsOpCode(OpCodes.Nop))),
                        Instruction.Create(OpCodes.Br_S, instructions.Last(x => x.IsOpCode(OpCodes.Ret))));
                }
            }

            var newInstructions = new List<Instruction>();

            // Step 2: OnMyFieldChanged callbacks
            //
            // IL_0007: ldarg.0      // this
            // IL_0008: call instance void Catel.Fody.TestAssembly.ObservableObjectTest_Expected/*0200004A*/::OnFirstNameChanged()/*060001C0*/

            var changeCallbackReference = _propertyData.ChangeCallbackReference;
            if (changeCallbackReference is not null)
            {
                newInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                newInstructions.Add(Instruction.Create(OpCodes.Call, changeCallbackReference));
            }

            // Step 3: RaisePropertyChanged
            //
            // IL_0007: ldarg.0      // this
            // IL_0008: ldstr        "ExistingProperty"
            // IL_000d: call instance void [Catel.Core/*23000003*/]Catel.Data.ObservableObject/*01000046*/::RaisePropertyChanged(string)/*0A000067*/

            newInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            newInstructions.Add(Instruction.Create(OpCodes.Ldstr, property.Name));
            newInstructions.Add(Instruction.Create(OpCodes.Call, _catelType.RaisePropertyChangedInvoker));

            // Final step: inject the instructions

            var instructionIndex = instructions.Count - 1;

            for (var i = instructionIndex; i >= 0; i++)
            {
                var instruction = instructions[i];
                if (instruction.IsOpCode(OpCodes.Ret))
                {
                    break;
                }

                instructionIndex--;
            }

            instructions.Insert(instructionIndex, newInstructions);

            body.OptimizeMacros();
        }
    }
}
