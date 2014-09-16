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
        public static readonly Dictionary<string, ArgumentMethodCallWeaverBase> WellKnownWeavers = CacheHelper.GetCache<Dictionary<string, ArgumentMethodCallWeaverBase>>("ArgumentMethodCallWeaverBase");

        private readonly TypeDefinition _argumentTypeDefinition = FodyEnvironment.ModuleDefinition.FindType("Catel.Core", "Catel.Argument");
        #endregion

        #region Methods
        public void Execute(TypeDefinition type, MethodDefinition methodDefinition, ParameterDefinition parameter, CustomAttribute attribute)
        {
            MethodDefinition selectedMethod;

            SelectMethod(_argumentTypeDefinition, parameter, out selectedMethod);
            if (selectedMethod != null)
            {
                var moduleDefinition = type.Module;
                var importedMethod = moduleDefinition.Import(selectedMethod);

                var instructions = new List<Instruction>();
                BuildInstructions(moduleDefinition, type, methodDefinition, parameter, attribute, instructions);
                if (importedMethod.HasGenericParameters)
                {
                    var genericInstanceMethod = new GenericInstanceMethod(importedMethod);
                    genericInstanceMethod.GenericArguments.Add(parameter.ParameterType);
                    instructions.Add(Instruction.Create(OpCodes.Call, genericInstanceMethod));
                }
                else
                {
                    instructions.Add(Instruction.Create(OpCodes.Call, importedMethod));
                }

                methodDefinition.Body.Instructions.Insert(0, instructions);        
            }
        }

        protected abstract void BuildInstructions(ModuleDefinition module, TypeDefinition type, MethodDefinition method, ParameterDefinition parameter, CustomAttribute attribute, List<Instruction> instructions);

        protected abstract void SelectMethod(TypeDefinition argumentTypeDefinition, ParameterDefinition parameter, out MethodDefinition selectedMethod);
        #endregion
    }
}