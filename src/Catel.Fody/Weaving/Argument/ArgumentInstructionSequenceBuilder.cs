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
            return BuildRegexRelatedInstructionsInternal(parameter, attribute);
        }

        public static IEnumerable<Instruction> BuildRegexRelatedInstructions(FieldDefinition field, CustomAttribute attribute)
        {
            return BuildRegexRelatedInstructionsInternal(field, attribute);
        }

        private static IEnumerable<Instruction> BuildRegexRelatedInstructionsInternal(object parameterDefinitionOrFieldDefinition, CustomAttribute attribute)
        {
            var pattern = (string)attribute.ConstructorArguments[0].Value;
            var regexOptions = (RegexOptions)attribute.ConstructorArguments[1].Value;

            foreach (var instruction in BuildDefaultInstructionsInternal(parameterDefinitionOrFieldDefinition))
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

            if (parameter.ParameterType.IsBoxingRequired(parameter.ParameterType))
            {
                yield return Instruction.Create(OpCodes.Box, parameter.ParameterType.Import());
            }
        }

        public static IEnumerable<Instruction> BuildDefaultInstructions(FieldDefinition field)
        {
            yield return Instruction.Create(OpCodes.Ldstr, field.Name);
            yield return Instruction.Create(OpCodes.Ldarg_0);
            yield return Instruction.Create(OpCodes.Ldfld, field);

            if (field.FieldType.IsBoxingRequired(field.FieldType))
            {
                yield return Instruction.Create(OpCodes.Box, field.FieldType.Import());
            }
        }

        private static IEnumerable<Instruction> BuildDefaultInstructionsInternal(object parameterDefinitionOrFieldDefinition)
        {
            if (parameterDefinitionOrFieldDefinition is FieldDefinition fieldDefinition)
            {
                return BuildDefaultInstructions(fieldDefinition);
            }

            var parameterDefinition = parameterDefinitionOrFieldDefinition as ParameterDefinition;
            if (parameterDefinitionOrFieldDefinition is not null)
            {
                return BuildDefaultInstructions(parameterDefinition);
            }

            return null;
        }

        public static IEnumerable<Instruction> BuildTypeCheckRelatedInstructions(ModuleDefinition module, object parameterDefinitionOrFieldDefinition, CustomAttribute attribute)
        {
            var typeReference = (TypeReference)attribute.ConstructorArguments[0].Value;
            foreach (var instruction in BuildDefaultInstructionsInternal(parameterDefinitionOrFieldDefinition))
            {
                yield return instruction;
            }

            var importedGetTypeFromHandle = module.ImportReference(module.GetMethod("GetTypeFromHandle"));

            yield return Instruction.Create(OpCodes.Ldtoken, typeReference);
            yield return Instruction.Create(OpCodes.Call, importedGetTypeFromHandle);
        }

        public static IEnumerable<Instruction> BuildBoundariesCheckInstructions(object parameterDefinitionOrFieldDefinition, CustomAttribute attribute)
        {
            foreach (var instruction in BuildDefaultInstructionsInternal(parameterDefinitionOrFieldDefinition))
            {
                yield return instruction;
            }

            foreach (var argument in attribute.ConstructorArguments)
            {
                var value = argument.Value;
                if (value is string s)
                {
                    yield return Instruction.Create(OpCodes.Ldstr, s);
                }
                else if (value is int i)
                {
                    yield return Instruction.Create(OpCodes.Ldc_I4, i);
                }
                else if (value is long)
                {
                    foreach (var instruction in BuildLongInstructions(value))
                    {
                        yield return instruction;
                    }
                }
                else if (value is float f)
                {
                    yield return Instruction.Create(OpCodes.Ldc_R4, f);
                }
                else if (value is double d)
                {
                    yield return Instruction.Create(OpCodes.Ldc_R8, d);
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
