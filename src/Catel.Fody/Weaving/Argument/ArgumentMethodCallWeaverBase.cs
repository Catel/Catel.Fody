// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentMethodCallWeaverBase.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Argument
{
    using System.Collections.Generic;

    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public abstract class ArgumentMethodCallWeaverBase
    {
        #region Constants
        public static readonly Dictionary<string, ArgumentMethodCallWeaverBase> WellKnownWeavers = new Dictionary<string, ArgumentMethodCallWeaverBase>();

        private static readonly TypeDefinition ArgumentTypeDefinition = FodyEnvironment.ModuleDefinition.FindType("Catel.Core", "Catel.Argument");
        #endregion

        #region Methods
        public void Execute(TypeDefinition type, MethodDefinition methodDefinition, ParameterDefinition parameter, CustomAttribute attribute)
        {
            MethodDefinition selectedMethod;

            SelectMethod(ArgumentTypeDefinition, out selectedMethod);
            var importedMethod = type.Module.Import(selectedMethod);

            var instructions = new List<Instruction>();
            BuildInstructions(type, methodDefinition, parameter, attribute, instructions);

            instructions.Add(Instruction.Create(OpCodes.Call, importedMethod));

            methodDefinition.Body.Instructions.Insert(0, instructions);
        }

        protected abstract void BuildInstructions(TypeDefinition type, MethodDefinition methodDefinition, ParameterDefinition parameter, CustomAttribute attribute, List<Instruction> instructions);

        protected abstract void SelectMethod(TypeDefinition argumentTypeDefinition, out MethodDefinition selectedMethod);
        #endregion
    }
}