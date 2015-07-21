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

    public partial class ArgumentWeaver
    {
        #region Constants
        private delegate CustomAttribute ExpressionToAttributeFunc(MethodReference method, IList<Instruction> instructions, Instruction instruction);
        #endregion

        #region Fields
        private static readonly Dictionary<string, ExpressionToAttributeFunc> ExpressionChecksToAttributeMappings = new Dictionary<string, ExpressionToAttributeFunc>();

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
                    else if (attributeFullName.StartsWith("Catel.Fody"))
                    {
                        FodyEnvironment.LogError(string.Format("Weaving of parameter '{0}' of methods '{1}' with attribute '{2}' is not (yet) supported, please use a different method",
                            method.GetFullName(), parameter.Name, attributeFullName));
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

                        if (!ExpressionChecksToAttributeMappings.ContainsKey(fullKey))
                        {
                            return;
                        }

                        var customAttribute = ExpressionChecksToAttributeMappings[fullKey](method, instructions, instruction);
                        if (customAttribute == null)
                        {
                            FodyEnvironment.LogWarning(string.Format("Can't find attribute for expression argument for method '{0}'",
                                method.GetFullName()));
                            continue;
                        }

                        var removedInfo = RemoveArgumentWeavingCall(method, instructions, instruction);
                        if (!displayClasses.Contains(removedInfo.Item1))
                        {
                            displayClasses.Add(removedInfo.Item1);
                        }

                        var weaver = ArgumentMethodCallWeaverBase.WellKnownWeavers[customAttribute.AttributeType.FullName];
                        if (!weaver.Execute(_typeDefinition, method, parameterOrField, customAttribute, removedInfo.Item2))
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
            lock (ExpressionChecksToAttributeMappings)
            {
                if (ExpressionChecksToAttributeMappings.Count > 0)
                {
                    return;
                }

                ExpressionChecksToAttributeMappings["Catel.Argument.IsNotNull"] = (m, ix, i) =>
                {
                    return CreateCustomAttribute("Catel.Fody.NotNullAttribute");
                };

                ExpressionChecksToAttributeMappings["Catel.Argument.IsNotNullOrEmpty"] = (m, ix, i) =>
                {
                    return CreateCustomAttribute("Catel.Fody.NotNullOrEmptyAttribute");
                };

                ExpressionChecksToAttributeMappings["Catel.Argument.IsNotNullOrWhitespace"] = (m, ix, i) =>
                {
                    return CreateCustomAttribute("Catel.Fody.NotNullOrWhitespaceAttribute");
                };

                // TODO: Add more

                //ExpressionChecksToAttributeMappings["Catel.Argument.IsNotOutOfRange"] = "Catel.Fody.NotOutOfRangeAttribute";

                ExpressionChecksToAttributeMappings["Catel.Argument.IsMinimal"] = (m, ix, i) =>
                {
                    // Previous operation is Ldc_[Something]
                    var previousInstruction = ix.GetPreviousInstruction(i);

                    return CreateCustomAttribute("Catel.Fody.MinimalAttribute", previousInstruction.Operand);
                };

                ExpressionChecksToAttributeMappings["Catel.Argument.IsMaximum"] = (m, ix, i) =>
                {
                    // Previous operation is Ldc_[Something]
                    var previousInstruction = ix.GetPreviousInstruction(i);

                    return CreateCustomAttribute("Catel.Fody.MaximumAttribute", previousInstruction.Operand);
                };
            }

            lock (ArgumentMethodCallWeaverBase.WellKnownWeavers)
            {
                if (ArgumentMethodCallWeaverBase.WellKnownWeavers.Count > 0)
                {
                    return;
                }

                ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotNullAttribute"] = new IsNotNullArgumentMethodCallWeaver();
                ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotNullOrEmptyAttribute"] = new IsNotNullOrEmptyArgumentMethodCallWeaver();
                ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotNullOrWhitespaceAttribute"] = new IsNotNullOrWhitespaceArgumentMethodCallWeaver();

                ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotNullOrEmptyArrayAttribute"] = new IsNotNullOrEmptyArrayArgumentMethodCallWeaver();
                ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.MatchAttribute"] = new IsMatchArgumentMethodCallWeaver();
                ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotMatchAttribute"] = new IsNotMatchArgumentMethodCallWeaver();
                ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.OfTypeAttribute"] = new IsOfTypeArgumentMethodCallWeave();
                ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.ImplementsInterfaceAttribute"] = new ImplementsInterfaceArgumentMethodCallWeave();
                ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.InheritsFromAttribute"] = new InheritsFromArgumentMethodCallWeaver();

                ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.NotOutOfRangeAttribute"] = new IsNotOutOfRangeMethodCallWeaver();
                ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.MinimalAttribute"] = new IsMinimalMethodCallWeaver();
                ArgumentMethodCallWeaverBase.WellKnownWeavers["Catel.Fody.MaximumAttribute"] = new IsMaximumMethodCallWeaver();
            }
        }
        #endregion
    }
}