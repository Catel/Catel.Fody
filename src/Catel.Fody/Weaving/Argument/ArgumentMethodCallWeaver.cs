// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentMethodCallWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Argument
{
    using System.Collections.Generic;

    using Mono.Cecil;
    using Mono.Cecil.Rocks;

    public abstract class ArgumentMethodCallWeaver
    {
        #region Constants
        public static readonly Dictionary<string, ArgumentMethodCallWeaver> WellKnownWeavers = new Dictionary<string, ArgumentMethodCallWeaver>();
        #endregion

        #region Constructors
        protected ArgumentMethodCallWeaver()
        {
            ArgumentTypeDefinition = FodyEnvironment.ModuleDefinition.FindType("Catel.Core", "Catel.Argument");
        }
        #endregion

        #region Properties
        protected TypeDefinition ArgumentTypeDefinition
        {
            get;
            private set;
        }
        #endregion

        #region Methods
        public void Execute(TypeDefinition type, MethodDefinition methodDefinition, ParameterDefinition parameter, CustomAttribute customAttribute)
        {
            methodDefinition.Body.SimplifyMacros();

            OnExecute(type, methodDefinition, parameter, customAttribute);

            methodDefinition.Body.OptimizeMacros();
        }

        protected abstract void OnExecute(TypeDefinition type, MethodDefinition methodDefinition, ParameterDefinition parameter, CustomAttribute customAttribute);
        #endregion
    }
}