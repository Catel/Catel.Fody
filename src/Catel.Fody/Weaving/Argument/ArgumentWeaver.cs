// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Argument
{
    using Mono.Cecil;
    using Mono.Cecil.Rocks;

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
            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotNullAttribute"] = new IsNotNullArgumentMethodCallWeaver();
            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotNullOrEmptyAttribute"] = new IsNotNullOrEmptyArgumentMethodCallWeaver();
            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotNullOrWhitespaceAttribute"] = new IsNotNullOrWhitespaceArgumentMethodCallWeaver();
            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotNullOrEmptyArrayAttribute"] = new IsNotNullOrEmptyArrayArgumentMethodCallWeaver();
            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.MatchAttribute"] = new IsMatchArgumentMethodCallWeaver();
            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotMatchAttribute"] = new IsNotMatchArgumentMethodCallWeaver();
            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.OfTypeAttribute"] = new IsOfTypeArgumentMethodCallWeave();
            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.ImplementsInterfaceAttribute"] = new ImplementsInterfaceArgumentMethodCallWeave();
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
            bool hasSimplifiedMethod = false;

            for (int i = method.Parameters.Count - 1; i >= 0; i--)
            {
                var parameter = method.Parameters[i];
                for (int j = parameter.CustomAttributes.Count - 1; j >= 0; j--)
                {
                    var customAttribute = parameter.CustomAttributes[j];
                    string attributeFullName = customAttribute.AttributeType.FullName;
                    if (ArgumentMethodCallWeaverBase.WellKnownWeavers.ContainsKey(attributeFullName))
                    {
                        if (!hasSimplifiedMethod)
                        {
                            method.Body.SimplifyMacros();
                            hasSimplifiedMethod = true;
                        }

                        ArgumentMethodCallWeaverBase.WellKnownWeavers[attributeFullName].Execute(_typeDefinition, method, parameter, customAttribute);
                        parameter.RemoveAttribute(attributeFullName);
                    }
                }
            }

            if (hasSimplifiedMethod)
            {
                method.Body.OptimizeMacros();
            }
        }
        #endregion
    }
}