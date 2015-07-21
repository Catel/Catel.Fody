// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstructionExtensions.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Catel.Fody
{
    using System.Collections.Generic;
    using System.Linq;

    using Mono.Cecil.Cil;

    public static class InstructionExtensions
    {
        public static bool IsOpCode(this Instruction instruction, params OpCode[] opCodes)
        {
            foreach (var opCode in opCodes)
            {
                if (instruction.OpCode == opCode)
                {
                    return true;
                }
            }

            return false;
        }

        public static SequencePoint GetSequencePoint(this IEnumerable<Instruction> instructions)
        {
            return instructions.Select(x => x.SequencePoint).FirstOrDefault(y => y != null);
        }
    }
}