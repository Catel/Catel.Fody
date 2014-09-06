// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Argument
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;
    using Mono.Collections.Generic;

    public partial class ArgumentWeaver
    {
        #region Constants
        #endregion

        #region Fields
        private readonly TypeDefinition _typeDefinition;
        private readonly MsCoreReferenceFinder _msCoreReferenceFinder;
        #endregion

        #region Constructors
        static ArgumentWeaver()
        {
            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotNullAttribute"] = new IsNotNullArgumentMethodCallWeaver();
            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Argument.IsNotNull"] = new IsNotNullArgumentMethodCallWeaver();

            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotNullOrEmptyAttribute"] = new IsNotNullOrEmptyArgumentMethodCallWeaver();
            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Argument.IsNotNullOrEmpty"] = new IsNotNullOrEmptyArgumentMethodCallWeaver();

            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotNullOrWhitespaceAttribute"] = new IsNotNullOrWhitespaceArgumentMethodCallWeaver();
            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Argument.NotNullOrWhitespace"] = new IsNotNullOrWhitespaceArgumentMethodCallWeaver();

            // TODO: Support the argument checks below in expression checks
            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotNullOrEmptyArrayAttribute"] = new IsNotNullOrEmptyArrayArgumentMethodCallWeaver();
            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.MatchAttribute"] = new IsMatchArgumentMethodCallWeaver();
            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotMatchAttribute"] = new IsNotMatchArgumentMethodCallWeaver();
            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.OfTypeAttribute"] = new IsOfTypeArgumentMethodCallWeave();
            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.ImplementsInterfaceAttribute"] = new ImplementsInterfaceArgumentMethodCallWeave();
            ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.InheritsFromAttribute"] = new InheritsFromArgumentMethodCallWeaver();

            //ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotOutOfRangeAttribute"] = new IsNotOutOfRangeMethodCallWeaver();
            //ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.MinimalAttribute"] = new IsMinimalMethodCallWeaver();
            //ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.MaximumAttribute"] = new IsMaximumMethodCallWeaver();
        }

        public ArgumentWeaver(TypeDefinition typeDefinition, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            _typeDefinition = typeDefinition;
            _msCoreReferenceFinder = msCoreReferenceFinder;
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
            method.Body.SimplifyMacros();
            var instructions = method.Body.Instructions;

            // Step 1) Convert attributes
            for (var i = method.Parameters.Count - 1; i >= 0; i--)
            {
                var parameter = method.Parameters[i];
                for (var j = parameter.CustomAttributes.Count - 1; j >= 0; j--)
                {
                    var customAttribute = parameter.CustomAttributes[j];
                    var attributeFullName = customAttribute.AttributeType.FullName;
                    if (ArgumentMethodCallWeaverBase.WellKnownWeavers.ContainsKey(attributeFullName))
                    {
                        ArgumentMethodCallWeaverBase.WellKnownWeavers[attributeFullName].Execute(_typeDefinition, method, parameter, customAttribute);
                        parameter.RemoveAttribute(attributeFullName);
                    }
                }
            }

            // Step 2) Convert expressions to normal calls
            var displayClasses = new List<TypeDefinition>();

            // Go backwards to keep the order of the arguments correct (because argument checks are injected at the beginnen of the ctor
            for (var i = instructions.Count - 1; i >=0; i--)
            {
                var instruction = instructions[i];
                if (IsSupportedExpressionArgumentCheck(instruction))
                {
                    var fullKey = ((MethodReference)instruction.Operand).GetFullName();
                    var parameter = GetParameterForExpressionArgumentCheck(method, instructions, instruction);
                    var customAttribute = CreateAttributeForExpressionArgumentCheck(method, instructions, instruction);

                    var displayClass = RemoveArgumentWeavingCall(method, instructions, instruction);
                    if (displayClass != null)
                    {
                        displayClasses.Add(displayClass);

                        ArgumentMethodCallWeaverBase.WellKnownWeavers[fullKey].Execute(_typeDefinition, method, parameter, customAttribute);

                        // Reset counter, start from the beginning
                        i = instructions.Count - 1;
                    }
                }
            }

            // Step 3) Clean up unnecessary code
            if (displayClasses.Count > 0)
            {
                instructions.RemoveSubsequentNops();

                foreach (var displayClass in displayClasses)
                {
                    RemoveObsoleteCodeForArgumentExpression(method, instructions, displayClass);
                }
            }

            method.Body.OptimizeMacros();
        }
        #endregion
    }
}