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
            if (parameterDefinition is not null)
            {
                targetType = parameterDefinition.ParameterType;
            }

            var fieldDefinition = parameterDefinitionOrFieldDefinition as FieldDefinition;
            if (fieldDefinition is not null)
            {
                targetType = fieldDefinition.FieldType;
            }

            if (targetType is not null)
            {
                try
                {
                    SelectMethod(_argumentTypeDefinition, targetType, out selectedMethod);
                }
                catch (Exception ex)
                {
                    var error = $"[{type.FullName}.{methodDefinition.Name}] {ex.Message}";

                    var sequencePoint = methodDefinition.GetFirstSequencePoint();
                    if (sequencePoint is not null)
                    {
                        FodyEnvironment.WriteErrorPoint(error, sequencePoint);
                    }
                    else
                    {
                        FodyEnvironment.WriteError(error);
                    }

                    return false;
                }
            }

            if (selectedMethod is null)
            {
                return false;
            }

            var moduleDefinition = type.Module;
            var importedMethod = moduleDefinition.ImportReference(selectedMethod);

            var instructions = new List<Instruction>();

            if (parameterDefinition is not null)
            {
                BuildInstructions(moduleDefinition, type, methodDefinition, parameterDefinition, attribute, instructions);
            }

            if (fieldDefinition is not null)
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
