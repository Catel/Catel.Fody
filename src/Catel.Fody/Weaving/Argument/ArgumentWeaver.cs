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
        private static readonly Dictionary<string, Action<TypeDefinition, CustomAttribute, MethodBody, ParameterDefinition, int>> ArgumentWeaveActions;
        private static readonly TypeDefinition ArgumentTypeDefinition;

        private readonly TypeDefinition _typeDefinition;
        private readonly ModuleDefinition _moduleDefinition;
        #endregion

        #region Constructors
        static ArgumentWeaver()
        {
            ArgumentWeaveActions = new Dictionary<string, Action<TypeDefinition, CustomAttribute, MethodBody, ParameterDefinition, int>>();
            ArgumentWeaveActions["Catel.Fody.NotNullAttribute"] = NotNullWeave;

            ArgumentTypeDefinition = FodyEnvironment.ModuleDefinition.FindType("Catel.Core", "Catel.Argument");
        }

        public ArgumentWeaver(TypeDefinition typeDefinition)
        {
            _typeDefinition = typeDefinition;
            _moduleDefinition = typeDefinition.Module;
        }
        #endregion

        #region Methods
        // TODO: Create a weaver class instead this and register it into the dictionary.
        private static void NotNullWeave(TypeDefinition type, CustomAttribute customAttribute, MethodBody body, ParameterDefinition parameter, int parameterIndex)
        {
            var methodDefinition = ArgumentTypeDefinition.Methods.FirstOrDefault(definition => definition.Name == "IsNotNull" && definition.Parameters.Count == 2);

            // Note: you need to import methods in other classes so it knows what assembly / type to call
            var importedMethod = type.Module.Import(methodDefinition);

            Collection<Instruction> instructions = body.Instructions;
            instructions.Insert(0, 
                //Instruction.Create(OpCodes.Nop), // Only required in debug mode
                Instruction.Create(OpCodes.Ldstr, parameter.Name), 
                Instruction.Create(OpCodes.Ldarg_S, parameter),
                Instruction.Create(OpCodes.Call, importedMethod));
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
            var methodBody = method.Body;
            methodBody.SimplifyMacros();

            int parameterIdx = 0;
            foreach (var parameter in method.Parameters)
            {
                for (int i = 0; i < parameter.CustomAttributes.Count; i++)
                //for (int i = parameter.CustomAttributes.Count - 1; i >= 0; i--)
                {
                    var customAttribute = parameter.CustomAttributes[i];
                    string attributeFullName = customAttribute.AttributeType.FullName;
                    if (ArgumentWeaveActions.ContainsKey(attributeFullName))
                    {
                        ArgumentWeaveActions[attributeFullName].Invoke(_typeDefinition, customAttribute, method.Body, parameter, parameterIdx);
                        parameter.RemoveAttribute(attributeFullName);
                    }
                }

                parameterIdx++;
            }

            methodBody.OptimizeMacros();
        }

        #endregion
    }
}