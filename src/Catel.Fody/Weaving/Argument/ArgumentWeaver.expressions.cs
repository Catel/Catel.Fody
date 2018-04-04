// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentWeaver.expressions.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Weaving.Argument
{
    using System;
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
            var methodBeingCalled = instruction.Operand as MethodReference;
            if (methodBeingCalled == null)
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

            FodyEnvironment.LogDebug($"Method '{method.GetFullName()}' no longer uses display class '{displayClassType.GetFullName()}', removing the display class from the method");

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

                                endIndex++;
                            }

                            for (var removeIndex = startIndex; removeIndex < endIndex; removeIndex++)
                            {
                                // Remove start index because it's our base address (and remove will move up everything)
                                instructions.RemoveAt(startIndex);
                            }
                        }
                    }
                }
            }

            // We need to remove usages of the constructor
            for (var i = 0; i < instructions.Count; i++)
            {
                var innerInstruction = instructions[i];
                if (innerInstruction.OpCode == OpCodes.Newobj)
                {
                    var remove = innerInstruction.UsesObjectFromDeclaringTypeName(displayClassType.Name);
                    if (remove)
                    {
                        // Delete 1 instruction, same location since remove will move everything 1 place up
                        instructions.RemoveAt(i);
                        //instructions.RemoveAt(i);

                        //if (instructions[i].OpCode == OpCodes.Dup)
                        //{
                        //    // Roslyn
                        //    instructions.RemoveAt(i);
                        //}
                        //else if ()
                        //{
                        //    // MsBuild
                        //    instructions.RemoveAt(i);
                        //}
                    }
                }
            }

            //// Remove all assignments to the display class
            ////   ldarg.0 
            ////   stfld class MyClass/<>c__DisplayClass0_0`1<!!T>::myArgument

            //for (var i = 0; i < instructions.Count; i++)
            //{
            //    var innerInstruction = instructions[i];
            //}

            // Remove display class allocation and assigments
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
                            var operand = instruction.Operand as VariableReference;
                            if (operand != null)
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

            // Remove display class from container
            var declaringType = displayClassType.DeclaringType;
            if (declaringType != null)
            {
                declaringType.NestedTypes.Remove(displayClassType);
            }

            // Remove display class - variables
            for (var i = 0; i < method.Body.Variables.Count; i++)
            {
                var variable = method.Body.Variables[i];
                if (string.Equals(variable.VariableType.Name, displayClassType.Name))
                {
                    method.Body.Variables.RemoveAt(i);
                    method.DebugInformation.Scope.Variables.RemoveAt(i);

                    i--;
                }
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
                                var operand = nextInstruction.Operand as MethodReference;
                                if (operand != null)
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
                    var methodReference = innerInstruction.Operand as MethodReference;
                    if (methodReference != null)
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
                        var fieldReference = innerInstruction.Operand as FieldReference;
                        if (fieldReference != null)
                        {
                            fieldDefinition = fieldReference.Resolve();
                        }
                    }

                    if (fieldDefinition == parameterFieldDefinition)
                    {
                        // We found it, now check 1 instruction back for the actual parameter
                        var finalInstruction = instructions[i - 1];

                        var finalFieldDefinition = finalInstruction.Operand as FieldDefinition;
                        if (finalFieldDefinition != null)
                        {
                            return finalFieldDefinition;
                        }

                        var parameterDefinition = finalInstruction.Operand as ParameterDefinition;
                        if (parameterDefinition != null)
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
            var fieldDefinition = instruction.Operand as FieldDefinition;
            if (fieldDefinition != null)
            {
                return fieldDefinition;
            }

            var fieldReference = instruction.Operand as FieldReference;
            if (fieldReference != null)
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