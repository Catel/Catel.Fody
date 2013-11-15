// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Argument
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;
    using Mono.Collections.Generic;

    public class ArgumentWeaver
    {
        #region Fields
        private static readonly Dictionary<string, Action<CustomAttribute, MethodBody, ParameterDefinition, int>> ArgumentWaveActions = new Dictionary<string, Action<CustomAttribute, MethodBody, ParameterDefinition, int>> { { "Catel.Fody.NotNullAttribute", NotNullWeave } };
 
        private readonly TypeDefinition _typeDefinition;
        #endregion

        #region Constructors
        public ArgumentWeaver(TypeDefinition typeDefinition)
        {
            _typeDefinition = typeDefinition;
        }
        #endregion

        #region Methods
        /// <summary>
        /// TODO: Create a weaver class instaed this and register it into the dictionary.
        /// </summary>
        /// <param name="customAttribute"></param>
        /// <param name="body"></param>
        /// <param name="parameter"></param>
        /// <param name="parameterIdx"></param>
        private static void NotNullWeave(CustomAttribute customAttribute, MethodBody body, ParameterDefinition parameter, int parameterIdx)
        {
            TypeDefinition argumentTypeDefinition = FodyEnvironment.ModuleDefinition.GetType("Catel.Argument");
            MethodDefinition methodDefinition = argumentTypeDefinition.Methods.FirstOrDefault(definition => definition.Name == "IsNotNull" && definition.Parameters.Count == 2);

            body.SimplifyMacros();

            Collection<Instruction> instructions = body.Instructions;
            instructions.Insert(0, Instruction.Create(OpCodes.Nop), Instruction.Create(OpCodes.Ldstr, parameter.Name), Instruction.Create(OpCodes.Ldarg_S, parameterIdx), Instruction.Create(OpCodes.Call, methodDefinition));

            body.OptimizeMacros();
        }

        public void Execute()
        {
            foreach (var method in _typeDefinition.Methods)
            {
                ProcessMethod(method);
            }
        }

        private void ProcessMethod(MethodDefinition method)
        {
            int parameterIdx = 0;
            foreach (ParameterDefinition parameter in method.Parameters)
            {
                foreach (CustomAttribute customAttribute in parameter.CustomAttributes)
                {
                    string attributeFullName = customAttribute.AttributeType.FullName;
                    if (ArgumentWaveActions.ContainsKey(attributeFullName))
                    {
                        ArgumentWaveActions[attributeFullName].Invoke(customAttribute, method.Body, parameter, parameterIdx);
                        parameter.RemoveAttribute(attributeFullName);
                    }
                }

                parameterIdx++;
            }
        }

        #endregion
    }
}