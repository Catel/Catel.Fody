// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentWeaver.expressions.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Weaving.Argument
{
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;
    using Mono.Collections.Generic;

    public partial class ArgumentWeaver
    {
        private bool IsSupportedExpressionArgumentCheck(MethodDefinition method, Instruction instruction)
        {
            if (!(instruction.Operand is MethodReference methodBeingCalled))
            {
                return false;
            }

            if (!methodBeingCalled.DeclaringType.FullName.Contains("Catel.Argument"))
            {
                return false;
            }

            var parameters = methodBeingCalled.Parameters;
            if (parameters.Count == 0)
            {
                return false;
            }

            var firstParameter = parameters[0];
            if (firstParameter == null)
            {
                return false;
            }

            if (!firstParameter.ParameterType.FullName.Contains("System.Linq.Expressions."))
            {
                return false;
            }

            var finalKey = methodBeingCalled.GetFullName();
            if (!ExpressionChecksToAttributeMappings.ContainsKey(finalKey))
            {
                FodyEnvironment.LogWarningPoint($"Expression argument method transformation in '{method.GetFullName()}' to '{methodBeingCalled.GetFullName()}' is not (yet) supported. To ensure the best performance, either rewrite this into a non-expression argument check or create a PR for Catel.Fody to enable support :-)", method.GetSequencePoint(instruction));

                return false;
            }

            return true;
        }

        private void RemoveObsoleteCodeForArgumentExpression(MethodDefinition method, Collection<Instruction> instructions, TypeDefinition displayClassType)
        {
            // Display class is used when there are still calls to load a field from the display class
            if (instructions.UsesType(displayClassType, OpCodes.Ldfld, OpCodes.Ldftn))
            {
                return;
            }

            var isAsyncMethod = method.IsAsyncMethod();
            if (isAsyncMethod)
            {
                // Too complex for now, see https://github.com/Catel/Catel.Fody/issues/33
                FodyEnvironment.LogWarning($"Method '{method.GetFullName()}' should no longer use display class '{displayClassType.GetFullName()}', but optimization for async methods has been turned off for now (see https://github.com/Catel/Catel.Fody/issues/33 for details)");
                return;
            }

            FodyEnvironment.LogDebug($"Method '{method.GetFullName()}' no longer uses display class '{displayClassType.GetFullName()}', removing the display class from the method");

            // Remove special constructors
            if (method.IsConstructor)
            {
                // We need to delete from the newobj => call to base constructor:
                //   L_000c: ldarg.0
                //   L_000d: call instance void [mscorlib]System.Object::.ctor()
                for (var i = 0; i < instructions.Count; i++)
                {
                    var innerInstruction = instructions[i];
                    if (innerInstruction.OpCode == OpCodes.Newobj)
                    {
                        var remove = innerInstruction.UsesObjectFromDeclaringTypeName(displayClassType.Name);
                        if (remove)
                        {
                            var startIndex = i;
                            var endIndex = i;

                            for (var j = i + 1; j < instructions.Count; j++)
                            {
                                var nextInstruction = instructions[j];
                                if (nextInstruction.IsOpCode(OpCodes.Ldarg, OpCodes.Ldarg_0))
                                {
                                    var nextNextInstruction = instructions[j + 1];
                                    if (nextNextInstruction.IsCallToMethodName(".ctor") ||
                                        nextNextInstruction.IsCallToMethodName(".cctor"))
                                    {
                                        break;
                                    }
                                }

                                endIndex = j;
                            }

                            instructions.RemoveInstructionsFromPositions(startIndex, endIndex);
                        }
                    }
                }
            }

            // Remove display class creation, can be either:
            //
            // Msbuild
            //   L_0000: newobj instance void Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass/<>c__DisplayClass1a::.ctor()
            //   L_0005: stloc.0
            //
            // Roslyn
            //   L_0000: newobj instance void Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass/<>c__DisplayClass1a::.ctor()
            //   L_0005: dup
            //
            // Async methods (note that in release mode, the compiler won't generate a field for the display class, and we need to
            // support both debug and release mode):
            //
            //   DEBUG MODE (display class stored in a field)
            //     L_0000: ldarg.0
            //     L_0001: ldfld int32 Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass /< CheckForNullAsync > d__16::<> 1__state
            //     L_0006: stloc.0
            //     L_0007: ldarg.0
            //     L_0008: newobj instance void Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass /<> c__DisplayClass16_0::.ctor()
            //     L_000d: stfld class Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass/<>c__DisplayClass16_0 Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass/<CheckForNullAsync>d__16::<>8__1
            //     L_0012: ldarg.0
            //     L_0013: ldfld class Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass/<>c__DisplayClass16_0 Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass/<CheckForNullAsync>d__16::<>8__1
            //     L_0018: ldarg.0
            //     L_0019: ldfld object Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass/<CheckForNullAsync>d__16::myObject
            //     L_001e: stfld object Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass/<>c__DisplayClass16_0::myObject
            //     L_0023: nop
            //
            //   RELEASE MODE (display class not stored in a field)
            //     L_0000: newobj instance void Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass /<> c__DisplayClass16_0::.ctor()
            //     L_0005: dup
            //     L_0006: ldarg.0
            //     L_0007: ldfld object Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass /< CheckForNullAsync > d__16::myObject
            //     L_000c: stfld object Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass /<> c__DisplayClass16_0::myObject
            //     L_0011: ldtoken Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass /<> c__DisplayClass16_0
            //     L_0016: call class [mscorlib] System.Type[mscorlib] System.Type::GetTypeFromHandle(valuetype[mscorlib] System.RuntimeTypeHandle)
            //     L_001b: call class [System.Core] System.Linq.Expressions.ConstantExpression[System.Core] System.Linq.Expressions.Expression::Constant(object, class [mscorlib] System.Type)
            //     L_0020: ldtoken object Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass/<>c__DisplayClass16_0::myObject
            //     L_0025: call class [mscorlib] System.Reflection.FieldInfo[mscorlib] System.Reflection.FieldInfo::GetFieldFromHandle(valuetype[mscorlib] System.RuntimeFieldHandle)
            //     L_002a: call class [System.Core] System.Linq.Expressions.MemberExpression[System.Core] System.Linq.Expressions.Expression::Field(class [System.Core] System.Linq.Expressions.Expression, class [mscorlib] System.Reflection.FieldInfo)
            //     L_002f: call !!0[] [mscorlib] System.Array::Empty<class [System.Core] System.Linq.Expressions.ParameterExpression>()
            //     L_0034: call class [System.Core] System.Linq.Expressions.Expression`1<!!0> [System.Core] System.Linq.Expressions.Expression::Lambda<class [mscorlib] System.Func`1<object>>(class [System.Core] System.Linq.Expressions.Expression, class [System.Core] System.Linq.Expressions.ParameterExpression[])
            //     L_0039: call void[Catel.Core] Catel.Argument::IsNotNull<object>(class [System.Core] System.Linq.Expressions.Expression`1<class [mscorlib] System.Func`1<!!0>>)
            //     L_003e: leave.s L_0057

            for (var i = 0; i < instructions.Count; i++)
            {
                var innerInstruction = instructions[i];
                if (innerInstruction.OpCode == OpCodes.Newobj)
                {
                    var remove = innerInstruction.UsesObjectFromDeclaringTypeName(displayClassType.Name);
                    if (remove)
                    {
                        var startIndex = i;
                        var endIndex = i;

                        // If the next instruction is stloc, remove that one as well
                        if (!isAsyncMethod)
                        {
                            var nextIndex = i + 1;
                            if (nextIndex < instructions.Count)
                            {
                                if (instructions[nextIndex].IsOpCode(OpCodes.Stloc, OpCodes.Stloc_0))
                                {
                                    endIndex = nextIndex;
                                }
                            }
                        }
                        else
                        {
                            // In async methods, we need to delete more since the values are stored in a field
                            var previousIndex = i - 1;
                            if (previousIndex >= 0)
                            {
                                if (instructions[previousIndex].IsOpCode(OpCodes.Ldarg, OpCodes.Ldarg_0))
                                {
                                    startIndex--;
                                }
                            }

                            // Search
                            var fieldOfDisplayClass = method.DeclaringType.Fields.FirstOrDefault(x => x.FieldType.Name.Equals(displayClassType.Name));
                            if (fieldOfDisplayClass != null)
                            {
                                // Async in DEBUG mode
                                for (var j = i + 1; j < instructions.Count - 1; j++)
                                {
                                    var currentInstruction = instructions[j];
                                    var nextInstruction = instructions[j + 1];

                                    if (currentInstruction.UsesField(fieldOfDisplayClass))
                                    {
                                        endIndex = j;

                                        if (nextInstruction.IsOpCode(OpCodes.Ldarg, OpCodes.Ldarg_0))
                                        {
                                            endIndex = j + 1;

                                            // Skip next instruction check, just handled it
                                            j++;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                // Async in RELEASE mode
                                for (var j = i + 1; j < instructions.Count - 1; j++)
                                {
                                    var currentInstruction = instructions[j];
                                    if (currentInstruction.IsOpCode(OpCodes.Dup))
                                    {
                                        endIndex = j;

                                        continue;
                                    }

                                    var nextInstruction = instructions[j + 1];

                                    if (currentInstruction.UsesObjectFromDeclaringTypeName(displayClassType.Name))
                                    {
                                        endIndex = j;

                                        if (nextInstruction.IsOpCode(OpCodes.Ldarg, OpCodes.Ldarg_0))
                                        {
                                            endIndex = j + 1;

                                            // Skip next instruction check, just handled it
                                            j++;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        instructions.RemoveInstructionsFromPositions(startIndex, endIndex);
                    }
                }
            }

            // Remove assignments in async methods
            //   Option A:
            //     L_0012: ldarg.0
            //     L_0013: ldfld class Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass/<>c__DisplayClass18_0 Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass/<CheckForNullAsync_MultipleParameters>d__18::<>8__1
            //
            //   Option B:
            //     L_0018: ldarg.0
            //     L_0019: ldfld object Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass/<CheckForNullAsync_MultipleParameters>d__18::myObject1
            //     L_001e: stfld object Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass/<>c__DisplayClass18_0::myObject1
            if (isAsyncMethod)
            {
                var fieldOfDisplayClass = method.DeclaringType.Fields.FirstOrDefault(x => x.FieldType.Name.Equals(displayClassType.Name));

                for (var i = 1; i < instructions.Count - 2; i++)
                {
                    var instruction = instructions[i];
                    if (instruction.IsOpCode(OpCodes.Ldfld))
                    {
                        var startIndex = i;
                        var endIndex = i;

                        if (fieldOfDisplayClass != null && instruction.UsesField(fieldOfDisplayClass))
                        {
                            // Option A
                            endIndex = i;

                            var previousInstruction = instructions[i - 1];
                            if (previousInstruction.IsOpCode(OpCodes.Ldarg))
                            {
                                startIndex = i - 1;
                            }
                        }

                        var nextInstruction = instructions[i + 1];
                        if (nextInstruction.UsesType(displayClassType, OpCodes.Stfld))
                        {
                            // Option B
                            endIndex = i + 1;

                            var previousInstruction = instructions[i - 1];
                            if (previousInstruction.IsOpCode(OpCodes.Ldarg))
                            {
                                startIndex = i - 1;
                            }
                        }

                        if (endIndex > startIndex)
                        {
                            instructions.RemoveInstructionsFromPositions(startIndex, endIndex);

                            // Always reset
                            i = 1;
                        }
                    }
                }
            }

            // Remove display class allocation and assignments
            //   L_0014: ldloc.0 (can also be dup)
            //   L_0015: ldarg.3
            //   L_0016: stfld object Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass/<>c__DisplayClass28::myObject3

            for (var i = 0; i < instructions.Count; i++)
            {
                var innerInstruction = instructions[i];
                if (innerInstruction.UsesType(displayClassType, OpCodes.Stfld))
                {
                    // Remove the stfld + 2 previous operations
                    instructions.RemoveAt(i);

                    if (i > 1)
                    {
                        instructions.RemoveAt(i - 1);
                    }

                    // Special case, we only need to remove when i - 2 is ldlock.s
                    var additionalIndex = i - 2;
                    if (additionalIndex >= 0)
                    {
                        var instruction = instructions[additionalIndex];
                        if (instruction.IsOpCode(OpCodes.Ldloc_S, OpCodes.Ldloc))
                        {
                            if (instruction.Operand is VariableReference operand)
                            {
                                var variableType = operand.VariableType;
                                if (variableType.IsGenericInstance)
                                {
                                    variableType = variableType.GetElementType();
                                }

                                if (variableType == displayClassType)
                                {
                                    instructions.RemoveAt(additionalIndex);
                                }
                            }
                        }
                    }

                    // Reset index & start over
                    i -= 3;

                    if (i < 0)
                    {
                        i = 0;
                    }
                }
            }

            // Remove display class loading
            for (var i = 0; i < instructions.Count; i++)
            {
                var innerInstruction = instructions[i];
                if (innerInstruction.UsesType(displayClassType, OpCodes.Ldtoken))
                {
                    instructions.RemoveAt(i--);
                }
            }

            // Remove display class - variables (regular methods)
            for (var i = 0; i < method.Body.Variables.Count; i++)
            {
                var variable = method.Body.Variables[i];
                if (string.Equals(variable.VariableType.Name, displayClassType.Name))
                {
                    method.Body.Variables.RemoveAt(i);

                    i--;
                }
            }

            // Remove display class - fields (async methods)
            for (var i = 0; i < method.DeclaringType.Fields.Count; i++)
            {
                var field = method.DeclaringType.Fields[i];
                if (string.Equals(field.FieldType.Name, displayClassType.Name))
                {
                    method.DeclaringType.Fields.RemoveAt(i);

                    i--;
                }
            }

            // Remove unused fields (clean up async methods that we have optimized, we don't check non-async because it
            // would require to check *all* methods of a class)
            if (isAsyncMethod)
            {
                for (var i = 0; i < method.DeclaringType.Fields.Count; i++)
                {
                    var methodDeclaringType = method.DeclaringType;
                    var field = method.DeclaringType.Fields[i];

                    if (!field.IsPrivate)
                    {
                        continue;
                    }

                    // Separate check for current method since this method is already simplified (and we don't want to optimize it yet)
                    if (instructions.UsesField(field))
                    {
                        continue;
                    }

                    var anyMethodUsesField = methodDeclaringType.Methods.Any(x =>
                    {
                        var usesField = false;

                        if (!x.Name.Equals(method.Name))
                        {
                            var body = x.Body;

                            body.SimplifyMacros();
                            usesField = body.Instructions.UsesField(field);
                            body.OptimizeMacros();
                        }

                        return usesField;
                    });

                    if (!anyMethodUsesField)
                    {


                        method.DeclaringType.Fields.RemoveAt(i);

                        i--;
                    }
                }
            }

            // Remove display class from container
            var declaringType = displayClassType.DeclaringType;
            if (declaringType != null)
            {
                declaringType.NestedTypes.Remove(displayClassType);
            }

            // Special case, remove any Dup opcodes before the argument checks
            for (var i = 0; i < instructions.Count; i++)
            {
                var remove = false;

                var innerInstruction = instructions[i];
                if (innerInstruction.IsOpCode(OpCodes.Dup))
                {
                    // If we have a non-expression argument call within 4 instructions, remove this one
                    for (var j = i + 1; j <= i + 5; j++)
                    {
                        if (j < instructions.Count)
                        {
                            var nextInstruction = instructions[j];
                            if (nextInstruction.IsOpCode(OpCodes.Call))
                            {
                                if (nextInstruction.Operand is MethodReference operand)
                                {
                                    if (operand.DeclaringType.Name.Contains("Argument") &&
                                        operand.Parameters[0].ParameterType.Name.Contains("String"))
                                    {
                                        remove = true;
                                    }
                                }
                            }
                        }
                    }
                }

                if (remove)
                {
                    instructions.RemoveAt(i--);
                }
            }

            // Remove duplicate nop instructions at the start of a method
            if (instructions.Count > 0)
            {
                var startInstruction = instructions[0];
                if (startInstruction.IsOpCode(OpCodes.Nop))
                {
                    instructions.RemoveAt(0);
                }
            }
        }

        private RemoveArgumentWeavingCallResult RemoveArgumentWeavingCall(MethodDefinition method, Collection<Instruction> instructions, Instruction instruction)
        {
            TypeReference displayClassType = null;

            var endRemoveIndex = instructions.IndexOf(instruction);
            var startRemoveIndex = endRemoveIndex;

            var hasBaseConstructorCall = false;
            var baseClass = method.DeclaringType.BaseType;

            for (var i = endRemoveIndex; i >= 0; i--)
            {
                // Remove everything until the first ldloc.0 call
                var innerInstruction = instructions[i];

                // CTL-908: never remove call to base constructor, inject any new argument checks after the ctor code
                if (innerInstruction.IsOpCode(OpCodes.Call))
                {
                    if (innerInstruction.Operand is MethodReference methodReference)
                    {
                        if (methodReference.Name == ".ctor" || methodReference.Name == ".cctor")
                        {
                            if (string.Equals(methodReference.DeclaringType.FullName, baseClass.FullName))
                            {
                                hasBaseConstructorCall = true;
                                startRemoveIndex = i + 2;
                                break;
                            }
                        }
                    }
                }

                // If this is a call to a new argument is not null, also stop processing (fix for https://github.com/Catel/Catel.Fody/issues/20)
                if (innerInstruction.UsesObjectFromDeclaringTypeName("Argument") &&
                    innerInstruction != instruction)
                {
                    break;
                }

                // From this point, the instruction can be removed
                startRemoveIndex = i;

                if (innerInstruction.IsOpCode(OpCodes.Ldtoken))
                {
                    if (displayClassType == null)
                    {
                        // First call to ldtoken with FieldDefinition contains the display class type
                        var fieldDefinition = GetFieldDefinition(innerInstruction);
                        if (fieldDefinition != null)
                        {
                            displayClassType = fieldDefinition.DeclaringType;
                        }
                    }
                }

                // Regular code
                if (innerInstruction.IsOpCode(OpCodes.Ldloc_0, OpCodes.Ldloc))
                {
                    break;
                }

                // Async/await code
                //
                //   ldarg.0
                //   ldfld
                if (innerInstruction.IsOpCode(OpCodes.Ldarg, OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3))
                {
                    var nextInstruction = instructions[i + 1];
                    if (nextInstruction.IsOpCode(OpCodes.Ldfld))
                    {
                        break;
                    }
                }

                // Since .NET core, we want to skip assignments:
                //
                //   ldarg.0
                //   stfld class MyClass/<>c__DisplayClass0_0`1<!!T>::myArgument
                //
                // If the display class is no longer used, another method must remove the code
                if (innerInstruction.IsOpCode(OpCodes.Ldarg, OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3))
                {
                    var nextInstruction = instructions[i + 1];
                    if (nextInstruction.IsOpCode(OpCodes.Stfld))
                    {
                        break;
                    }
                }

                // .net core code
                if (innerInstruction.IsOpCode(OpCodes.Dup))
                {
                    break;
                }
            }

            // Remove at the end so we have access to next / previous instructions
            for (var i = endRemoveIndex; i >= startRemoveIndex; i--)
            {
                instructions.RemoveAt(i);
            }

            return new RemoveArgumentWeavingCallResult(displayClassType.Resolve(), startRemoveIndex, hasBaseConstructorCall);
        }

        private object GetParameterOrFieldForExpressionArgumentCheck(MethodDefinition method, Collection<Instruction> instructions, Instruction instruction)
        {
            TypeReference displayClassType = null;
            FieldDefinition parameterFieldDefinition = null;

            // This logic is used for finding the parameter:

            // 1) Find the display class
            // 2) Find the field used
            // 3) Check with which parameter the field is assigned

            var index = instructions.IndexOf(instruction);
            for (var i = index; i >= 0; i--)
            {
                var innerInstruction = instructions[i];

                if (innerInstruction.IsOpCode(OpCodes.Ldtoken))
                {
                    if (displayClassType == null)
                    {
                        // First call to ldtoken with FieldDefinition contains the display class type
                        parameterFieldDefinition = GetFieldDefinition(innerInstruction);
                        if (parameterFieldDefinition != null)
                        {
                            displayClassType = parameterFieldDefinition.DeclaringType;
                        }
                    }
                }

                if (innerInstruction.IsOpCode(OpCodes.Stfld))
                {
                    var fieldDefinition = innerInstruction.Operand as FieldDefinition;
                    if (fieldDefinition == null)
                    {
                        if (innerInstruction.Operand is FieldReference fieldReference)
                        {
                            fieldDefinition = fieldReference.Resolve();
                        }
                    }

                    if (fieldDefinition == parameterFieldDefinition)
                    {
                        // We found it, now check 1 instruction back for the actual parameter
                        var finalInstruction = instructions[i - 1];

                        if (finalInstruction.Operand is FieldDefinition finalFieldDefinition)
                        {
                            return finalFieldDefinition;
                        }

                        if (finalInstruction.Operand is ParameterDefinition parameterDefinition)
                        {
                            return parameterDefinition;
                        }
                    }
                }
            }

            return null;
        }

        private FieldDefinition GetFieldDefinition(Instruction instruction)
        {
            // First call to ldtoken with FieldDefinition contains the display class type
            if (instruction.Operand is FieldDefinition fieldDefinition)
            {
                return fieldDefinition;
            }

            if (instruction.Operand is FieldReference fieldReference)
            {
                return fieldReference.Resolve();
            }

            return null;
        }

        private CustomAttribute CreateCustomAttribute(string attributeTypeName, params object[] parameters)
        {
            var typeDefinition = _typeDefinition.Module.FindType("Catel.Fody.Attributes", attributeTypeName);

            var typeList = new List<TypeDefinition>();
            foreach (var parameter in parameters)
            {
                var parameterType = parameter.GetType();
                if (parameterType.IsClass)
                {
                    parameterType = typeof(object);
                }

                typeList.Add(_msCoreReferenceFinder.GetCoreTypeReference(parameterType.FullName).Resolve());
            }

            var constructor = typeDefinition.FindConstructor(typeList).Resolve();
            var attribute = new CustomAttribute(constructor);
            for (var i = 0; i < constructor.Parameters.Count; i++)
            {
                attribute.ConstructorArguments.Add(new CustomAttributeArgument(typeList[i], parameters[i]));
            }

            return attribute;
        }
    }
}
