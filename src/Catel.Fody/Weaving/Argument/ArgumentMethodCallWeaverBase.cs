// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentMethodCallWeaverBase.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Argument
{
    using System;
    using System.Collections.Generic;

    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public abstract class ArgumentMethodCallWeaverBase
    {
        #region Constants
        public static readonly Dictionary<string, ArgumentMethodCallWeaverBase> WellKnownWeavers = CacheHelper.GetCache<Dictionary<string, ArgumentMethodCallWeaverBase>>("ArgumentMethodCallWeaverBase");

        private readonly TypeDefinition _argumentTypeDefinition = FodyEnvironment.ModuleDefinition.FindType("Catel.Core", "Catel.Argument");
        #endregion

        #region Methods
        public bool Execute(TypeDefinition type, MethodDefinition methodDefinition, 
            object parameterDefinitionOrFieldDefinition, CustomAttribute attribute, int instructionIndex)
        {
            TypeReference targetType = null;
            MethodDefinition selectedMethod = null;

            var parameterDefinition = parameterDefinitionOrFieldDefinition as ParameterDefinition;
            if (parameterDefinition != null)
            {
                targetType = parameterDefinition.ParameterType;
            }

            var fieldDefinition = parameterDefinitionOrFieldDefinition as FieldDefinition;
            if (fieldDefinition != null)
            {
                targetType = fieldDefinition.FieldType;
            }

            if (targetType != null)
            {
                try
                {
                    SelectMethod(_argumentTypeDefinition, targetType, out selectedMethod);
                }
                catch (Exception ex)
                {
                    var error = $"[{type.FullName}.{methodDefinition.Name}] {ex.Message}";

                    var sequencePoint = methodDefinition.GetFirstSequencePoint();
                    if (sequencePoint != null)
                    {
                        FodyEnvironment.LogErrorPoint(error, sequencePoint);
                    }
                    else
                    {
                        FodyEnvironment.LogError(error);
                    }

                    return false;
                }
            }

            if (selectedMethod == null)
            {
                return false;
            }

            var moduleDefinition = type.Module;
            var importedMethod = moduleDefinition.Import(selectedMethod);

            var instructions = new List<Instruction>();

            if (parameterDefinition != null)
            {
                BuildInstructions(moduleDefinition, type, methodDefinition, parameterDefinition, attribute, instructions);
            }

            if (fieldDefinition != null)
            {
                BuildInstructions(moduleDefinition, type, methodDefinition, fieldDefinition, attribute, instructions);
            }

            if (importedMethod.HasGenericParameters)
            {
                var genericInstanceMethod = new GenericInstanceMethod(importedMethod);
                genericInstanceMethod.GenericArguments.Add(targetType);
                instructions.Add(Instruction.Create(OpCodes.Call, genericInstanceMethod));
            }
            else
            {
                instructions.Add(Instruction.Create(OpCodes.Call, importedMethod));
            }

            methodDefinition.Body.Instructions.Insert(instructionIndex, instructions);

            return true;
        }

        protected abstract void BuildInstructions(ModuleDefinition module, TypeDefinition type, MethodDefinition method, ParameterDefinition parameter, CustomAttribute attribute, List<Instruction> instructions);

        protected abstract void BuildInstructions(ModuleDefinition module, TypeDefinition type, MethodDefinition method, FieldDefinition field, CustomAttribute attribute, List<Instruction> instructions);

        protected abstract void SelectMethod(TypeDefinition argumentTypeDefinition, TypeReference typeToCheck, out MethodDefinition selectedMethod);
        #endregion
    }
}