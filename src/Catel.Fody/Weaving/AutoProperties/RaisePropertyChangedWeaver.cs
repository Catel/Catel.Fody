// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OnPropertyChangedWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Weaving.AutoProperties
{
    using System.Collections.Generic;
    using System.Linq;
    using Catel.Fody.Weaving.Argument;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

    internal class RaisePropertyChangedWeaver
    {
        #region Fields
        private readonly CatelType _catelType;

        private readonly MsCoreReferenceFinder _msCoreReferenceFinder;
        #endregion

        #region Constructors
        public RaisePropertyChangedWeaver(CatelType catelType, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            _catelType = catelType;
            _msCoreReferenceFinder = msCoreReferenceFinder;
        }
        #endregion

        #region Methods
        public void Execute()
        {
            FodyEnvironment.WriteDebug($"\tExecuting '{GetType().Name}' for '{_catelType.TypeDefinition.FullName}'");

            foreach (var method in _catelType.TypeDefinition.Methods)
            {
                FixRaisePropertyChangedMethod(method);
            }
        }

        private void FixRaisePropertyChangedMethod(MethodDefinition method)
        {
            var methodBody = method.Body;

            methodBody.SimplifyMacros();

            var instructions = methodBody.Instructions;

            for (var i = 0; i < instructions.Count; i++)
            {
                // ORIGINAL:
                // 
                // IL_0001: ldarg.0      // this
                // IL_0002: ldarg.0      // this
                // IL_0003: ldtoken      Catel.Fody.TestAssembly.ObservableObjectTest
                // IL_0008: call         class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
                // IL_000d: call         class [System.Core]System.Linq.Expressions.ConstantExpression [System.Core]System.Linq.Expressions.Expression::Constant(object, class [mscorlib]System.Type)
                // IL_0012: ldtoken      method instance string Catel.Fody.TestAssembly.ObservableObjectTest::get_ManualChangeNotificationProperty()
                // IL_0017: call         class [mscorlib]System.Reflection.MethodBase [mscorlib]System.Reflection.MethodBase::GetMethodFromHandle(valuetype [mscorlib]System.RuntimeMethodHandle)
                // IL_001c: castclass    [mscorlib]System.Reflection.MethodInfo
                // IL_0021: call         class [System.Core]System.Linq.Expressions.MemberExpression [System.Core]System.Linq.Expressions.Expression::Property(class [System.Core]System.Linq.Expressions.Expression, class [mscorlib]System.Reflection.MethodInfo)
                // IL_0026: call         !!0/*class [System.Core]System.Linq.Expressions.ParameterExpression*/[] [mscorlib]System.Array::Empty<class [System.Core]System.Linq.Expressions.ParameterExpression>()
                // IL_002b: call         class [System.Core]System.Linq.Expressions.Expression`1<!!0/*class [mscorlib]System.Func`1<string>*/> [System.Core]System.Linq.Expressions.Expression::Lambda<class [mscorlib]System.Func`1<string>>(class [System.Core]System.Linq.Expressions.Expression, class [System.Core]System.Linq.Expressions.ParameterExpression[])
                // IL_0030: call         instance void [Catel.Core]Catel.Data.ObservableObject::RaisePropertyChanged<string>(class [System.Core]System.Linq.Expressions.Expression`1<class [mscorlib]System.Func`1<!!0/*string*/>>)
                // IL_0035: nop

                // REPLACED:
                //
                // IL_0001: ldarg.0      // this
                // IL_0002: ldstr        "ManualChangeNotificationProperty"
                // IL_0007: call         instance void [Catel.Core]Catel.Data.ObservableObject::RaisePropertyChanged(string)

                var instruction = instructions[i];
                if (!instruction.IsOpCode(OpCodes.Call))
                {
                    continue;
                }

                var genericInstanceMethod = instruction.Operand as GenericInstanceMethod;
                if (genericInstanceMethod is null)
                {
                    continue;
                }

                if (genericInstanceMethod.Name != "RaisePropertyChanged")
                {
                    continue;
                }

                var startInstruction = i;
                var propertyName = string.Empty;
                for (var j = i; j >= 0; j--)
                {
                    var potentialInstruction = instructions[j];
                    if (potentialInstruction.IsOpCode(OpCodes.Ldtoken))
                    {
                        var methodDefinition = potentialInstruction.Operand as MethodDefinition;
                        if (methodDefinition != null)
                        {
                            var name = methodDefinition.Name;
                            if (name.StartsWith("get_"))
                            {
                                propertyName = name.Replace("get_", string.Empty);
                                startInstruction = j;
                                break;
                            }
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    // Not found, cannot optimize
                    continue;
                }

                while (startInstruction >= 0)
                {
                    startInstruction--;

                    var potentialInstruction = instructions[startInstruction];
                    if (potentialInstruction.IsOpCode(OpCodes.Ldtoken))
                    {
                        if (potentialInstruction.Operand is TypeDefinition)
                        {
                            // Found it, remove another 1 for ldarg
                            startInstruction--;

                            if (startInstruction > 0 && instructions[startInstruction].IsOpCode(OpCodes.Ldarg, OpCodes.Ldarg_0))
                            {
                                // Remove another one
                                startInstruction--;
                            }
                            break;
                        }
                    }
                }

                if (startInstruction <= 0)
                {
                    // Not found, cannot optimize
                    continue;
                }

                FodyEnvironment.WriteDebug($"Optimizing 'RaisePropertyChanged(() => {propertyName})' to 'RaisePropertyChanged(\"{propertyName}\")' in '{method.GetFullName()}'");

                instructions.RemoveInstructionsFromPositions(startInstruction, i);

                var newInstructions = new List<Instruction>(new[] {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldstr, propertyName),
                    Instruction.Create(OpCodes.Call, _catelType.RaisePropertyChangedInvoker)
                });

                instructions.Insert(startInstruction, newInstructions.ToArray());
            }

            methodBody.OptimizeMacros();
        }

        private MethodReference GetSystemObjectEqualsMethodReference(ModuleDefinition moduleDefinition)
        {
            var typeReference = _msCoreReferenceFinder.GetCoreTypeReference("System.String");
            var typeDefinition = typeReference.Resolve();

            var methodDefinition = typeDefinition.Methods.Single(m => m.Name == "Equals" && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.Name == "String");
            return methodDefinition;
        }
        #endregion
    }
}
