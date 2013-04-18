// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RecursiveIlFinder.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody
{
    using System.Collections.Generic;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public class RecursiveIlFinder
    {
        private readonly TypeDefinition _typeDefinition;
        private readonly List<MethodDefinition> _processedMethods;
        public List<Instruction> Instructions;

        public RecursiveIlFinder(TypeDefinition typeDefinition)
        {
            _typeDefinition = typeDefinition;
            Instructions = new List<Instruction>();
            _processedMethods = new List<MethodDefinition>();
        }

        public void Execute(MethodDefinition getMethod)
        {
            _processedMethods.Add(getMethod);
            if (getMethod.Body == null)
            {
                return;
            }
            foreach (var instruction in getMethod.Body.Instructions)
            {
                Instructions.Add(instruction);
                if (!IsCall(instruction.OpCode))
                {
                    continue;
                }
                var methodDefinition = instruction.Operand as MethodDefinition;
                if (methodDefinition == null)
                {
                    continue;
                }
                if (methodDefinition.IsGetter || methodDefinition.IsSetter)
                {
                    continue;
                }
                if (_processedMethods.Contains(methodDefinition))
                {
                    continue;
                }
                if (methodDefinition.DeclaringType != _typeDefinition)
                {
                    continue;
                }
                Execute(methodDefinition);
            }
        }

        private static bool IsCall(OpCode opCode)
        {
            return opCode == OpCodes.Call ||
                   opCode == OpCodes.Callvirt ||
                   opCode == OpCodes.Calli ||
                   opCode == OpCodes.Ldftn;
        }
    }
}