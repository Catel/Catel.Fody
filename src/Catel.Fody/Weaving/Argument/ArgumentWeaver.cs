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
        public ArgumentWeaver(TypeDefinition typeDefinition, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            EnsureCache();

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
            if (method.Body == null)
            {
                return;
            }

            // Note: very important to only simplify/optimize methods that we actually change, otherwise some Mono.Cecil bugs
            // will appear on the surface
            Collection<Instruction> instructions = null;

            var methodFullName = method.GetFullName();
            FodyEnvironment.LogDebug(string.Format("Processing method '{0}'", methodFullName));

            // Step 1) Convert attributes
            // TODO: how to handle async/await here?
            for (var i = method.Parameters.Count - 1; i >= 0; i--)
            {
                var parameter = method.Parameters[i];
                for (var j = parameter.CustomAttributes.Count - 1; j >= 0; j--)
                {
                    var customAttribute = parameter.CustomAttributes[j];
                    var attributeFullName = customAttribute.AttributeType.FullName;
                    if (ArgumentMethodCallWeaverBase.WellKnownWeavers.ContainsKey(attributeFullName))
                    {
                        if (instructions == null)
                        {
                            method.Body.SimplifyMacros();
                            instructions = method.Body.Instructions;
                        }

                        ArgumentMethodCallWeaverBase.WellKnownWeavers[attributeFullName].Execute(_typeDefinition, method, parameter, customAttribute, 0);
                        parameter.RemoveAttribute(attributeFullName);
                    }
                }
            }

            // Step 2) Convert expressions to normal calls
            var displayClasses = new List<TypeDefinition>();

            // Go backwards to keep the order of the arguments correct (because argument checks are injected at the beginnen of the ctor)
            if (instructions != null || ContainsArgumentChecks(method))
            {
                if (instructions == null)
                {
                    method.Body.SimplifyMacros();
                    instructions = method.Body.Instructions;
                }

                for (var i = instructions.Count - 1; i >= 0; i--)
                {
                    var instruction = instructions[i];
                    if (IsSupportedExpressionArgumentCheck(instruction))
                    {
                        var fullKey = ((MethodReference)instruction.Operand).GetFullName();
                        var parameterOrField = GetParameterOrFieldForExpressionArgumentCheck(method, instructions, instruction);
                        if (parameterOrField == null)
                        {
                            FodyEnvironment.LogWarning(string.Format("Cannot weave at least one argument of method '{0}'", method.GetFullName()));
                            continue;
                        }

                        var customAttribute = CreateAttributeForExpressionArgumentCheck(method, instructions, instruction);

                        var removedInfo = RemoveArgumentWeavingCall(method, instructions, instruction);
                        if (!displayClasses.Contains(removedInfo.Item1))
                        {
                            displayClasses.Add(removedInfo.Item1);
                        }

                        if (!ArgumentMethodCallWeaverBase.WellKnownWeavers[fullKey].Execute(_typeDefinition, method, parameterOrField,
                            customAttribute, removedInfo.Item2))
                        {
                            // We failed, the build should fail now
                            return;
                        }

                        // Reset counter, start from the beginning
                        i = instructions.Count - 1;
                    }
                }

                // Step 3) Clean up unnecessary code
                if (displayClasses.Count > 0)
                {
                    foreach (var displayClass in displayClasses)
                    {
                        RemoveObsoleteCodeForArgumentExpression(method, instructions, displayClass);
                    }
                }
            }

            if (instructions != null)
            {
                method.Body.OptimizeMacros();
            }
        }

        private bool ContainsArgumentChecks(MethodDefinition method)
        {
            foreach (var instruction in method.Body.Instructions)
            {
                var methodReference = instruction.Operand as MethodReference;
                if (methodReference != null)
                {
                    if (methodReference.GetFullName().Contains("Catel.Argument"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void EnsureCache()
        {
            lock (ArgumentMethodCallWeaverBase.WellKnownWeavers)
            {
                if (ArgumentMethodCallWeaverBase.WellKnownWeavers.Count > 0)
                {
                    return;
                }

                ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotNullAttribute"] = new IsNotNullArgumentMethodCallWeaver();
                ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Argument.IsNotNull"] = new IsNotNullArgumentMethodCallWeaver();

                ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotNullOrEmptyAttribute"] = new IsNotNullOrEmptyArgumentMethodCallWeaver();
                ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Argument.IsNotNullOrEmpty"] = new IsNotNullOrEmptyArgumentMethodCallWeaver();

                ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotNullOrWhitespaceAttribute"] = new IsNotNullOrWhitespaceArgumentMethodCallWeaver();
                ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Argument.IsNotNullOrWhitespace"] = new IsNotNullOrWhitespaceArgumentMethodCallWeaver();

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
        }
        #endregion
    }
}