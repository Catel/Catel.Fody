// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentInstructionSequenceBuilder.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Argument
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    using Mono.Cecil;
    using Mono.Cecil.Cil;

    internal static class ArgumentInstructionSequenceBuilder
    {
        #region Methods
        public static IEnumerable<Instruction> BuildRegexRelatedInstructions(ParameterDefinition parameter, CustomAttribute attribute)
        {
            var pattern = (string)attribute.ConstructorArguments[0].Value;
            var regexOptions = (RegexOptions)attribute.ConstructorArguments[1].Value;

            foreach (var instruction in BuildDefaultInstructions(parameter))
            {
                yield return instruction;
            }

            yield return Instruction.Create(OpCodes.Ldstr, pattern);
            yield return Instruction.Create(OpCodes.Ldc_I4, (int)regexOptions);
        }

        public static IEnumerable<Instruction> BuildDefaultInstructions(ParameterDefinition parameter)
        {
            yield return Instruction.Create(OpCodes.Ldstr, parameter.Name);
            yield return Instruction.Create(OpCodes.Ldarg_S, parameter);

            if (parameter.ParameterType.IsBoxingRequired())
            {
                yield return Instruction.Create(OpCodes.Box, parameter.ParameterType.Import());
            }
        }

        public static IEnumerable<Instruction> BuildTypeCheckRelatedInstructions(ModuleDefinition module, ParameterDefinition parameter, CustomAttribute attribute)
        {
            var typeReference = (TypeReference)attribute.ConstructorArguments[0].Value;
            foreach (var instruction in BuildDefaultInstructions(parameter))
            {
                yield return instruction;
            }

            var importedGetTypeFromHandle = module.Import(module.GetMethod("GetTypeFromHandle"));

            yield return Instruction.Create(OpCodes.Ldtoken, typeReference);
            yield return Instruction.Create(OpCodes.Call, importedGetTypeFromHandle);
        }

        public static IEnumerable<Instruction> BuildBoundariesCheckInstructions(ParameterDefinition parameter, CustomAttribute attribute)
        {
            foreach (var instruction in BuildDefaultInstructions(parameter))
            {
                yield return instruction;
            }

            foreach (var argument in attribute.ConstructorArguments)
            {
                object value = argument.Value;
                if (value is string)
                {
                    yield return Instruction.Create(OpCodes.Ldstr, (string)value);
                }
                else if (value is int)
                {
                    yield return Instruction.Create(OpCodes.Ldc_I4, (int)value);
                }
                else if (value is long)
                {
                    foreach (var instruction in BuildLongInstructions(value))
                    {
                        yield return instruction;
                    }
                }
                else if (value is float)
                {
                    yield return Instruction.Create(OpCodes.Ldc_R4, (float)value);
                }
                else if (value is double)
                {
                    yield return Instruction.Create(OpCodes.Ldc_R8, (double)value);
                }
            }
        }

        private static IEnumerable<Instruction> BuildLongInstructions(object minValue)
        {
            if ((long)minValue <= int.MaxValue)
            {
                // Note: don't use Ldc_I8 here, although it is a long
                yield return Instruction.Create(OpCodes.Ldc_I4, (int)(long)minValue);
                yield return Instruction.Create(OpCodes.Conv_I8);
            }
            else
            {
                yield return Instruction.Create(OpCodes.Ldc_I8, (long)minValue);
            }
        }
        #endregion
    }
}