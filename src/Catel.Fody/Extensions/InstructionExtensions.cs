// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstructionExtensions.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Catel.Fody
{
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
    }
}