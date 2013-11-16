// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicArgumentMethodCallWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Argument
{
    using System.Linq;

    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Collections.Generic;

    public abstract class BasicArgumentMethodCallWeaver : ArgumentMethodCallWeaver
    {
        #region Fields
        private readonly string _methodName;
        #endregion

        #region Constructors
        protected BasicArgumentMethodCallWeaver(string methodName)
        {
            _methodName = methodName;
        }
        #endregion

        #region Methods
        protected override void OnExecute(TypeDefinition type, MethodDefinition methodDefinition, ParameterDefinition parameter, CustomAttribute customAttribute)
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