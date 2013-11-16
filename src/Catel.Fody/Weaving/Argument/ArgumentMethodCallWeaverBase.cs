// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicArgumentMethodCallWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Argument
{
    using System.Collections.Generic;
    using System.Linq;

    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Collections.Generic;

    public abstract class ArgumentMethodCallWeaverBase
    {
        #region Constants
        public static readonly Dictionary<string, ArgumentMethodCallWeaverBase> WellKnownWeavers = new Dictionary<string, ArgumentMethodCallWeaverBase>();
        #endregion

        #region Fields
        private readonly string _methodName;
        #endregion

        #region Constructors
        protected ArgumentMethodCallWeaverBase(string methodName)
        {
            _methodName = methodName;

            ArgumentTypeDefinition = FodyEnvironment.ModuleDefinition.FindType("Catel.Core", "Catel.Argument");
        }
        #endregion

        #region Properties
        protected TypeDefinition ArgumentTypeDefinition { get; private set; }
        #endregion

        #region Methods
        public void Execute(TypeDefinition type, MethodDefinition methodDefinition, ParameterDefinition parameter, CustomAttribute customAttribute)
        {
            OnExecute(type, methodDefinition, parameter, customAttribute);
        }

        protected virtual void OnExecute(TypeDefinition type, MethodDefinition methodDefinition, ParameterDefinition parameter, CustomAttribute customAttribute)
        {
            var argumentMethodDefinitionToCall = ArgumentTypeDefinition.Methods.FirstOrDefault(definition => definition.Name == _methodName && definition.Parameters.Count == 2);

            var importedMethod = type.Module.Import(argumentMethodDefinitionToCall);

            Collection<Instruction> instructions = methodDefinition.Body.Instructions;
            instructions.Insert(0, Instruction.Create(OpCodes.Ldstr, parameter.Name),
                Instruction.Create(OpCodes.Ldarg_S, parameter),
                Instruction.Create(OpCodes.Call, importedMethod));
        }
        #endregion
    }
}