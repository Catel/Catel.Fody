// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BoundariesCheckRelatedArgumentMethodCallWeaverBase.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Argument
{
    using System.Collections.Generic;

    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public abstract class BoundariesCheckRelatedArgumentMethodCallWeaverBase : ArgumentMethodCallWeaverBase
    {
        #region Methods
        protected sealed override void BuildInstructions(ModuleDefinition module, TypeDefinition type, MethodDefinition method, ParameterDefinition parameter, CustomAttribute attribute, List<Instruction> instructions)
        {
            instructions.AddRange(ArgumentInstructionSequenceBuilder.BuildBoundariesCheckInstructions(parameter, attribute));
        }
        #endregion
    }
}