// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstructionExtensions.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Catel.Fody
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public static class InstructionExtensions
    {
        public static bool IsOpCode(this Instruction instruction, params OpCode[] opCodes)
        {
            if (opCodes.Length == 0)
            {
                return true;
            }

            foreach (var opCode in opCodes)
            {
                if (instruction.OpCode == opCode)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsCallToMethodName(this Instruction instruction, string methodName)
        {
            if (!instruction.IsOpCode(OpCodes.Call, OpCodes.Calli, OpCodes.Callvirt))
            {
                return false;
            }

            if (instruction.Operand is MethodReference methodReference)
            {
                if (string.Equals(methodReference.Name, methodName))
                {
                    return true;
                }
            }

            if (instruction.Operand is MethodDefinition methodDefinition)
            {
                if (string.Equals(methodDefinition.Name, methodName))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool UsesObjectFromDeclaringTypeName(this Instruction instruction, string declaringTypeName)
        {
            if (instruction.Operand is MethodReference methodReference)
            {
                if (string.Equals(methodReference.DeclaringType.Name, declaringTypeName))
                {
                    return true;
                }
            }

            if (instruction.Operand is MethodDefinition methodDefinition)
            {
                if (string.Equals(methodDefinition.DeclaringType.Name, declaringTypeName))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool UsesType(this Instruction instruction, TypeDefinition typeDefinition, params OpCode[] opCodes)
        {
            if (instruction.IsOpCode(opCodes))
            {
                if (instruction.Operand is FieldDefinition fieldDefinition)
                {
                    if (string.Equals(fieldDefinition.DeclaringType.Name, typeDefinition.Name))
                    {
                        return true;
                    }
                }

                if (instruction.Operand is FieldReference fieldReference)
                {
                    if (string.Equals(fieldReference.DeclaringType.Name, typeDefinition.Name))
                    {
                        return true;
                    }
                }

                if (instruction.Operand is MethodDefinition methodDefinition)
                {
                    if (string.Equals(methodDefinition.DeclaringType.Name, typeDefinition.Name))
                    {
                        return true;
                    }
                }

                if (instruction.Operand is MethodReference methodReference)
                {
                    if (string.Equals(methodReference.DeclaringType.Name, typeDefinition.Name))
                    {
                        return true;
                    }
                }

                if (instruction.Operand is TypeDefinition operandTypeDefinition)
                {
                    if (string.Equals(operandTypeDefinition.Name, typeDefinition.Name))
                    {
                        return true;
                    }
                }

                if (instruction.Operand is TypeReference operandTypeReference)
                {
                    if (string.Equals(operandTypeReference.Name, typeDefinition.Name))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool UsesField(this Instruction instruction, FieldDefinition fieldDefinition)
        {
            if (instruction.Operand is FieldDefinition usedFieldDefinition)
            {
                if (string.Equals(usedFieldDefinition.Name, fieldDefinition.Name))
                {
                    return true;
                }
            }

            if (instruction.Operand is FieldReference usedFieldReference)
            {
                if (string.Equals(usedFieldReference.Name, fieldDefinition.Name))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
