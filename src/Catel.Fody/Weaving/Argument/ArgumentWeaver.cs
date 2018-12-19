// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Argument
{
    using System.Collections.Generic;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;
    using Mono.Collections.Generic;

    public partial class ArgumentWeaver
    {
        #region Constants
        private delegate CustomAttribute ExpressionToAttributeFunc(MethodReference method, IList<Instruction> instructions, Instruction instruction);

        private static readonly object CacheLock = new object();
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
            if (method.Body is null)
            {
                return;
            }

            if (method.IsDecoratedWithAttribute("NoWeavingAttribute"))
            {
                FodyEnvironment.LogDebug($"\t\tSkipping '{method.Name}' because 'Catel.Fody.NoWeavingAttribute'");

                return;
            }

            // Note: very important to only simplify/optimize methods that we actually change, otherwise some Mono.Cecil bugs
            // will appear on the surface
            Collection<Instruction> instructions = null;

            var methodFullName = method.GetFullName();
            FodyEnvironment.LogDebug($"Processing method '{methodFullName}'");

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
                        if (instructions is null)
                        {
                            method.Body.SimplifyMacros();
                            instructions = method.Body.Instructions;
                        }

                        ArgumentMethodCallWeaverBase.WellKnownWeavers[attributeFullName].Execute(_typeDefinition, method, parameter, customAttribute, 0);
                        parameter.RemoveAttribute(attributeFullName);
                    }
                    else if (attributeFullName.StartsWith("Catel.Fody"))
                    {
                        FodyEnvironment.LogErrorPoint($"Weaving of parameter '{method.GetFullName()}' of methods '{parameter.Name}' with attribute '{attributeFullName}' is not (yet) supported, please use a different method", method.GetFirstSequencePoint());
                    }
                }
            }

            // Step 2) Convert expressions to normal calls
            var displayClasses = new List<TypeDefinition>();

            // Go backwards to keep the order of the arguments correct (because argument checks are injected at the beginnen of the ctor)
            if (instructions != null || ContainsArgumentChecks(method))
            {
                if (instructions is null)
                {
                    method.Body.SimplifyMacros();
                    instructions = method.Body.Instructions;
                }

                for (var i = instructions.Count - 1; i >= 0; i--)
                {
                    var instruction = instructions[i];
                    if (IsSupportedExpressionArgumentCheck(method, instruction))
                    {
                        var fullKey = ((MethodReference)instruction.Operand).GetFullName();
                        var parameterOrField = GetParameterOrFieldForExpressionArgumentCheck(method, instructions, instruction);
                        if (parameterOrField is null)
                        {
                            FodyEnvironment.LogWarning($"Cannot weave at least one argument of method '{method.GetFullName()}'");
                            continue;
                        }

                        if (!ExpressionChecksToAttributeMappings.ContainsKey(fullKey))
                        {
                            return;
                        }

                        var customAttribute = ExpressionChecksToAttributeMappings[fullKey](method, instructions, instruction);
                        if (customAttribute is null)
                        {
                            FodyEnvironment.LogWarningPoint($"Expression argument method transformation in '{method.GetFullName()}' to '{fullKey}' is not (yet) supported. To ensure the best performance, either rewrite this into a non-expression argument check or create a PR for Catel.Fody to enable support :-)", method.GetSequencePoint(instruction));

                            continue;
                        }

                        var removedInfo = RemoveArgumentWeavingCall(method, instructions, instruction);
                        if (!displayClasses.Contains(removedInfo.DisplayClassTypeDefinition))
                        {
                            displayClasses.Add(removedInfo.DisplayClassTypeDefinition);
                        }

                        var weaver = ArgumentMethodCallWeaverBase.WellKnownWeavers[customAttribute.AttributeType.FullName];
                        if (!weaver.Execute(_typeDefinition, method, parameterOrField, customAttribute, removedInfo.Index))
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

                // Step 4) Remove double nop commands, start at 1
                // Note: disabled because there might be jump codes to different Nop instructions
                //for (int i = 1; i < instructions.Count; i++)
                //{
                //    if (instructions[i].IsOpCode(OpCodes.Nop) && instructions[i - 1].IsOpCode(OpCodes.Nop))
                //    {
                //        instructions.RemoveAt(i--);
                //    }
                //}
            }

            if (instructions != null)
            {
                method.Body.OptimizeMacros();
                method.UpdateDebugInfo();
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
            lock (CacheLock)
            {
                EnsureExpressionChecksCache();
                EnsureWeaversCache();
            }
        }

        private void EnsureExpressionChecksCache()
        {
            lock (CacheLock)
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

                ExpressionChecksToAttributeMappings["Catel.Argument.IsNotOutOfRange"] = (m, ix, i) =>
                {
                    // Previous operations are Ldc_[Something]
                    var previousInstruction2 = ix.GetPreviousInstruction(i);
                    var previousInstruction1 = ix.GetPreviousInstruction(previousInstruction2);

                    if (!IsOperandSupportedForArgumentChecks(previousInstruction1.Operand))
                    {
                        return null;
                    }

                    return CreateCustomAttribute("Catel.Fody.NotOutOfRangeAttribute", previousInstruction1.Operand, previousInstruction2.Operand);
                };

                ExpressionChecksToAttributeMappings["Catel.Argument.IsMinimal"] = (m, ix, i) =>
                {
                    // Previous operation is Ldc_[Something]
                    var operand = ix.GetPreviousInstruction(i).Operand;
                    if (!IsOperandSupportedForArgumentChecks(operand))
                    {
                        return null;
                    }

                    return CreateCustomAttribute("Catel.Fody.MinimalAttribute", operand);
                };

                ExpressionChecksToAttributeMappings["Catel.Argument.IsMaximum"] = (m, ix, i) =>
                {
                    // Previous operation is Ldc_[Something]
                    var operand = ix.GetPreviousInstruction(i).Operand;
                    if (!IsOperandSupportedForArgumentChecks(operand))
                    {
                        return null;
                    }

                    return CreateCustomAttribute("Catel.Fody.MaximumAttribute", operand);
                };
            }
        }

        private void EnsureWeaversCache()
        {
            lock (CacheLock)
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

        private bool IsOperandSupportedForArgumentChecks(object operand)
        {
            if (operand == null)
            {
                return false;
            }

            // Ignore strings
            if (operand.GetType().FullName.Contains("System.String"))
            {
                return false;
            }

            return true;
        }
        #endregion
    }
}
