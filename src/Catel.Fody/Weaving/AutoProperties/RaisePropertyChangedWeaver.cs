namespace Catel.Fody.Weaving.AutoProperties
{
    using System.Collections.Generic;
    using System.Data;
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
            if (method.IsAbstract || method.IsStatic)
            {
                return;
            }

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

                var startInstructionIndex = i;
                var propertyName = string.Empty;
                for (var j = i; j >= 0; j--)
                {
                    var potentialInstruction = instructions[j];
                    if (potentialInstruction.IsOpCode(OpCodes.Ldtoken))
                    {
                        var methodDefinition = potentialInstruction.Operand as MethodDefinition;
                        if (methodDefinition is not null)
                        {
                            var name = methodDefinition.Name;
                            if (name.StartsWith("get_"))
                            {
                                propertyName = name.Replace("get_", string.Empty);
                                startInstructionIndex = j;
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

                while (startInstructionIndex >= 0)
                {
                    startInstructionIndex--;

                    var potentialInstruction = instructions[startInstructionIndex];
                    if (potentialInstruction.IsOpCode(OpCodes.Ldtoken))
                    {
                        if (potentialInstruction.Operand is TypeDefinition)
                        {
                            // Found it, remove another 1 for ldarg
                            startInstructionIndex--;

                            if (startInstructionIndex > 0 && instructions[startInstructionIndex].IsOpCode(OpCodes.Ldarg, OpCodes.Ldarg_0))
                            {
                                // Remove another one
                                startInstructionIndex--;
                            }
                            break;
                        }
                    }
                }

                if (startInstructionIndex <= 0)
                {
                    // Not found, cannot optimize
                    continue;
                }

                FodyEnvironment.WriteDebug($"Optimizing 'RaisePropertyChanged(() => {propertyName})' to 'RaisePropertyChanged(\"{propertyName}\")' in '{method.GetFullName()}'");

                var startInstruction = instructions[startInstructionIndex];

                // Find jump instructions we need to rewrite
                var jumpInstructions = new List<Instruction>();

                for (var j = 0; j <= startInstructionIndex; j++)
                {
                    var potentialInstruction = instructions[j];
                    // Note: we don't check the op code, as long as the instruction is the operand, we need to replace it
                    //if (potentialInstruction.IsOpCode(OpCodes.Brfalse, OpCodes.Brfalse_S, OpCodes.Brtrue, OpCodes.Brtrue_S, OpCodes.Leave, OpCodes.Leave_S))
                    {
                        var operand = potentialInstruction.Operand as Instruction;
                        if (operand == startInstruction)
                        {
                            // Replace!
                            jumpInstructions.Add(potentialInstruction);
                            //potentialInstruction.Operand = newInstructions.First();
                        }
                    }
                }

                // Start replacing
                instructions.RemoveInstructionsFromPositions(startInstructionIndex, i);

                var newInstructions = new List<Instruction>(new[] {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldstr, propertyName),
                    Instruction.Create(OpCodes.Call, _catelType.RaisePropertyChangedInvoker)
                });

                instructions.Insert(startInstructionIndex, newInstructions.ToArray());

                // Fix all potential returns
                foreach (var jumpInstruction in jumpInstructions)
                {
                    jumpInstruction.Operand = instructions[startInstructionIndex];
                }

                // Reset counter, we might have multiple calls
                i = startInstructionIndex;
            }

            methodBody.OptimizeMacros();
        }
        #endregion
    }
}
