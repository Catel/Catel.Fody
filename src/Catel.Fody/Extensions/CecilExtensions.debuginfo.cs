// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CecilExtensions.debuginfo.cs" company="Catel development team">
//   Copyright (c) 2008 - 2017 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using System.Linq;
    using System.Reflection;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Collections.Generic;

    public static partial class CecilExtensions
    {
        private const long AddressToIgnore = 16707566;

        private static readonly FieldInfo SequencePointOffsetFieldInfo = typeof(SequencePoint).GetField("offset", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo InstructionOffsetInstructionFieldInfo = typeof(InstructionOffset).GetField("instruction", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void UpdateDebugInfo(this MethodDefinition method)
        {
            var debugInfo = method.DebugInformation;
            var instructions = method.Body.Instructions;
            var scope = debugInfo.Scope;

            if (scope == null || instructions.Count == 0)
            {
                return;
            }

            var oldSequencePoints = debugInfo.SequencePoints;
            var newSequencePoints = new Collection<SequencePoint>();

            // Step 1: check if all variables are present
            foreach (var variable in method.Body.Variables)
            {
                if (method.IsAsyncMethod())
                {
                    // Skip some special items of an async method:
                    // 1) int (state?)
                    // 2) exception
                    if (variable.Index == 0 && variable.VariableType.Name.Contains("Int") ||
                        variable.VariableType.Name.Contains("Exception"))
                    {
                        continue;
                    }
                }

                if (!ContainsVariable(scope, variable))
                {
                    var variableDebugInfo = new VariableDebugInformation(variable, $"__var_{variable.Index}");
                    scope.Variables.Add(variableDebugInfo);
                }
            }

            // Step 2: Make sure the instructions point to the correct items
            foreach (var oldSequencePoint in oldSequencePoints)
            {
                //var isValid = false;

                //// Special cases we need to ignore
                //if (oldSequencePoint.StartLine == AddressToIgnore ||
                //    oldSequencePoint.EndLine == AddressToIgnore)
                //{
                //    continue;
                //}

                var instructionOffset = (InstructionOffset)SequencePointOffsetFieldInfo.GetValue(oldSequencePoint);
                var offsetInstruction = (Instruction)InstructionOffsetInstructionFieldInfo.GetValue(instructionOffset);

                // Fix offset
                for (var i = 0; i < instructions.Count; i++)
                {
                    var instruction = instructions[i];
                    if (instruction == offsetInstruction)
                    {
                        var newSequencePoint = new SequencePoint(instruction, oldSequencePoint.Document)
                        {
                            StartLine = oldSequencePoint.StartLine,
                            StartColumn = oldSequencePoint.StartColumn,
                            EndLine = oldSequencePoint.EndLine,
                            EndColumn = oldSequencePoint.EndColumn
                        };

                        newSequencePoints.Add(newSequencePoint);

                        //isValid = true;

                        break;
                    }
                }
            }

            debugInfo.SequencePoints.Clear();

            foreach (var newSequencePoint in newSequencePoints)
            {
                debugInfo.SequencePoints.Add(newSequencePoint);
            }

            // Step 3: Remove any unused variables
            RemoveUnusedVariablesFromDebugInfo(scope);

            // Final step: update the scopes by setting the indices
            scope.Start = new InstructionOffset(instructions.First());
            scope.End = new InstructionOffset(instructions.Last());
        }

        private static bool ContainsVariable(this ScopeDebugInformation debugInfo, VariableDefinition variable)
        {
            // Note: just checking for index might not be sufficient
            var hasVariable = debugInfo.Variables.Any(x => x.Index == variable.Index);
            if (hasVariable)
            {
                return true;
            }

            // Important: check nested scopes
            for (var i = 0; i < debugInfo.Scopes.Count; i++)
            {
                if (ContainsVariable(debugInfo.Scopes[i], variable))
                {
                    return true;
                }
            }

            return false;
        }

        private static void RemoveUnusedVariablesFromDebugInfo(this ScopeDebugInformation debugInfo)
        {
            // Remove any variables that no longer have a valid index (such as -1)
            for (var i = 0; i < debugInfo.Variables.Count; i++)
            {
                var debugInfoVariable = debugInfo.Variables[i];
                if (debugInfoVariable.Index < 0)
                {
                    debugInfo.Variables.Remove(debugInfoVariable);
                    i--;
                }
            }

            // Important: nested scopes (for example, for async methods)
            for (var i = 0; i < debugInfo.Scopes.Count; i++)
            {
                RemoveUnusedVariablesFromDebugInfo(debugInfo.Scopes[i]);
            }
        }
    }
}
