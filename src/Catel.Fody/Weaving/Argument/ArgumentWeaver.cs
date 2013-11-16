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
        #region Constants
        #endregion

        #region Fields
        private readonly TypeDefinition _typeDefinition;
        #endregion

        #region Constructors
        static ArgumentWeaver()
        {
            ArgumentMethodCallWeaver.WellKnownWeavers["Catel.Fody.NotNullAttribute"] = new IsNotNullArgumentMethodCallWeaver();
            ArgumentMethodCallWeaver.WellKnownWeavers["Catel.Fody.NotNullOrEmptyAttribute"] = new IsNotNullOrEmptyArgumentMethodCallWeaver();
            ArgumentMethodCallWeaver.WellKnownWeavers["Catel.Fody.NotNullOrWhitespaceAttribute"] = new IsNotNullOrWhitespaceArgumentMethodCallWeaver();
        }

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
            for (int i = method.Parameters.Count - 1; i >= 0; i--)
            {
                var parameter = method.Parameters[i];
                for (int j = parameter.CustomAttributes.Count - 1; j >= 0; j--)
                {
                    var customAttribute = parameter.CustomAttributes[j];
                    string attributeFullName = customAttribute.AttributeType.FullName;
                    if (ArgumentMethodCallWeaver.WellKnownWeavers.ContainsKey(attributeFullName))
                    {
                        ArgumentMethodCallWeaver.WellKnownWeavers[attributeFullName].Execute(_typeDefinition, method, parameter, customAttribute);
                        parameter.RemoveAttribute(attributeFullName);
                    }
                }
            }
        }
        #endregion
    }
}