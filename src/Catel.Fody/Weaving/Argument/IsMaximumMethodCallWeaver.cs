// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IsMaximumMethodCallWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Argument
{
    using System.Collections.Generic;
    using System.Linq;

    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public class IsMaximumMethodCallWeaver : ArgumentMethodCallWeaverBase
    {
        #region Methods
        protected override void BuildInstructions(ModuleDefinition module, TypeDefinition type, MethodDefinition method, ParameterDefinition parameter, CustomAttribute attribute, List<Instruction> instructions)
        {
            instructions.AddRange(ArgumentInstructionSequenceBuilder.BuildOutOfRangeInstructions(parameter, attribute));
        }

        protected override void SelectMethod(TypeDefinition argumentTypeDefinition, ParameterDefinition parameter, out MethodDefinition selectedMethod)
        {
            selectedMethod = argumentTypeDefinition.Methods.FirstOrDefault(definition => definition.Name == "IsMaximum" && definition.HasGenericParameters && definition.Parameters.Count == 3 && definition.Parameters[0].ParameterType.FullName == "System.String");
        }
        #endregion
    }
}