// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IsMatchArgumentMethodCallWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Argument
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public sealed class IsMatchArgumentMethodCallWeaver : ArgumentMethodCallWeaverBase
    {
        #region Methods
        protected override void BuildInstructions(TypeDefinition type, MethodDefinition methodDefinition, ParameterDefinition parameter, CustomAttribute attribute, List<Instruction> instructions)
        {
            var pattern = (string)attribute.ConstructorArguments[0].Value;
            var regexOptions = (RegexOptions)attribute.ConstructorArguments[1].Value;
            instructions.Add(Instruction.Create(OpCodes.Ldstr, parameter.Name));
            instructions.Add(Instruction.Create(OpCodes.Ldarg_S, parameter));
            instructions.Add(Instruction.Create(OpCodes.Ldstr, pattern));
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, (int)regexOptions));
        }

        protected override void SelectMethod(TypeDefinition argumentTypeDefinition, out MethodDefinition selectedMethod)
        {
            selectedMethod = argumentTypeDefinition.Methods.FirstOrDefault(definition => definition.Name == "IsMatch" && definition.Parameters.Count == 4);
        }
        #endregion
    }
}