namespace Catel.Fody.Weaving.Argument
{
    using System.Collections.Generic;

    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public abstract class DefaultArgumentMethodCallWeaverBase : ArgumentMethodCallWeaverBase
    {
        #region Methods
        protected override sealed void BuildInstructions(ModuleDefinition module, TypeDefinition type, MethodDefinition method, ParameterDefinition parameter, CustomAttribute attribute, List<Instruction> instructions)
        {
            instructions.AddRange(ArgumentInstructionSequenceBuilder.BuildDefaultInstructions(parameter));
        }

        protected override sealed void BuildInstructions(ModuleDefinition module, TypeDefinition type, MethodDefinition method, FieldDefinition field, CustomAttribute attribute, List<Instruction> instructions)
        {
            instructions.AddRange(ArgumentInstructionSequenceBuilder.BuildDefaultInstructions(field));
        }
        #endregion
    }
}