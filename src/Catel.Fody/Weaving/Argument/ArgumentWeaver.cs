// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Weaving.Argument
{
    using Mono.Cecil;

    public class ArgumentWeaver
    {
        #region Fields
        private readonly TypeDefinition _typeDefinition;
        #endregion

        #region Constructors
        public ArgumentWeaver(TypeDefinition typeDefinition)
        {
            _typeDefinition = typeDefinition;
        }
        #endregion

        #region Methods
        public void Execute()
        {
            foreach (var method in _typeDefinition.Methods)
            {
                ProcessMethod(method);
            }
        }

        private void ProcessMethod(MethodDefinition method)
        {
            // TODO: Implement
        }
        #endregion
    }
}