namespace Catel.Fody.Services
{
    using Mono.Cecil;

    public class ValidationService
    {
        private readonly ModuleWeaver _moduleWeaver;

        public ValidationService(ModuleWeaver moduleWeaver)
        {
            _moduleWeaver = moduleWeaver;
        }

        public void Validate()
        {
            _moduleWeaver.LogInfo("Validating correct usage of Catel.Fody");

            var types = _moduleWeaver.ModuleDefinition.Types;

            foreach (var type in types)
            {
                ValidateExcludeFromBackupAttributes(type);
                ValidateExcludeFromBackupAttributes(type);
            }
        }

        private void ValidateExcludeFromBackupAttributes(TypeDefinition typeDefinition)
        {
            ValidateAttributes(typeDefinition, "ExcludeFromBackup");
        }

        private void ValidateExposeAttributes(TypeDefinition typeDefinition)
        {
            ValidateAttributes(typeDefinition, "Expose");
        }

        private void ValidateAttributes(TypeDefinition typeDefinition, string attributeName)
        {
            // todo; check type itself, and fields

            foreach (var property in typeDefinition.Properties)
            {
                if (property.IsDecoratedWithAttribute($"{attributeName}Attribute"))
                {
                    _moduleWeaver.LogError($"[{typeDefinition.FullName}.{property.Name}] is decorated with the '{attributeName}' attribute, which is not allowed post-weaving, please double check correct usage");
                }
            }
        }
    }
}
