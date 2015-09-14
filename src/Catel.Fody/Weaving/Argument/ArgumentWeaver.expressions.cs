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

            var firstParameter = methodBeingCalled.Parameters.FirstOrDefault();
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
                FodyEnvironment.LogWarningPoint(string.Format("Expression argument method transformation in '{0}' to '{1}' is not (yet) supported. To ensure the best performance, either rewrite this into a non-expression argument check or create a PR for Catel.Fody to enable support :-)",
                     method.GetFullName(), methodBeingCalled.GetFullName()), method.Body.Instructions.GetSequencePoint(instruction));

                return false;
            }

            return true;
        }

        private void RemoveObsoleteCodeForArgumentExpression(MethodDefinition method, Collection<Instruction> instructions, TypeDefinition displayClassType)
        {
            // Display class is used when there are still calls to load a field from the display class
            if (instructions.UsesDisplayClass(displayClassType, OpCodes.Ldfld, OpCodes.Ldftn))
            {
                return;
            }

            FodyEnvironment.LogDebug(string.Format("Method '{0}' no longer uses display class '{1}', removing the display class from the method", method.GetFullName(),
                displayClassType.GetFullName()));

            // Remote display class from container
            if (method.DeclaringType.NestedTypes.Contains(displayClassType))
            {
                method.DeclaringType.NestedTypes.Remove(displayClassType);
            }

            // Remove display class - variables
            for (var i = 0; i < method.Body.Variables.Count; i++)
            {
                var variable = method.Body.Variables[i];
                if (string.Equals(variable.VariableType.Name, displayClassType.Name))
                {
                    method.Body.Variables.RemoveAt(i--);
                }
            }

            // Remove display class creation
            //L_0000: newobj instance void Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass/<>c__DisplayClass1a::.ctor()
            //L_0005: stloc.0 

            for (var i = 0; i < instructions.Count; i++)
            {
                var innerInstruction = instructions[i];
                if (innerInstruction.OpCode == OpCodes.Newobj)
                {
                    var remove = false;

                    var methodReference = innerInstruction.Operand as MethodReference;
                    if (methodReference != null)
                    {
                        if (string.Equals(methodReference.DeclaringType.Name, displayClassType.Name))
                        {
                            remove = true;
                        }
                    }

                    var methodDefinition = innerInstruction.Operand as MethodDefinition;
                    if (methodDefinition != null)
                    {
                        if (string.Equals(methodDefinition.DeclaringType.Name, displayClassType.Name))
                        {
                            remove = true;
                        }
                    }

                    if (remove)
                    {
                        // Delete 2 instructions, same location since remove will move everything 1 place up
                        instructions.RemoveAt(i);
                        instructions.RemoveAt(i);
                    }
                }
            }

            // Remove display class allocation
            //L_0014: ldloc.0 
            //L_0015: ldarg.3 
            //L_0016: stfld object Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass/<>c__DisplayClass28::myObject3

            for (var i = 0; i < instructions.Count; i++)
            {
                var innerInstruction = instructions[i];
                if (innerInstruction.UsesDisplayClass(displayClassType, OpCodes.Stfld))
                {
                    // Remove the stfld + 2 previous operations
                    instructions.RemoveAt(i);
                    instructions.RemoveAt(i - 1);
                    instructions.RemoveAt(i - 2);

                    i -= 3;
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

        private Tuple<TypeDefinition, int> RemoveArgumentWeavingCall(MethodDefinition method, Collection<Instruction> instructions, Instruction instruction)
        {
            TypeReference displayClassType = null;
            var index = instructions.IndexOf(instruction);

            for (var i = index; i >= 0; i--)
            {
                // Remove everything until the first ldloc.0 call
                var innerInstruction = instructions[i];

                instructions.RemoveAt(i);

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
                if (innerInstruction.IsOpCode(OpCodes.Ldarg, OpCodes.Ldarg_0))
                {
                    break;
                }

                index = i;
            }

            return new Tuple<TypeDefinition, int>(displayClassType.Resolve(), index - 1);
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