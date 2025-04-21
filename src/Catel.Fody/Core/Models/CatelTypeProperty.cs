namespace Catel.Fody
{
    using System.Diagnostics;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    [DebuggerDisplay("{Name}")]
    public class CatelTypeProperty
    {
        private readonly CatelType _catelType;

        public CatelTypeProperty(CatelType catelType, TypeDefinition typeDefinition, PropertyDefinition propertyDefinition)
        {
            _catelType = catelType;
            TypeDefinition = typeDefinition;
            PropertyDefinition = propertyDefinition;
            Name = propertyDefinition.Name;

            DetermineFields();
            DetermineMethods();
            DetermineDefaultValue();
            DetermineIncludeInBackup();
            DetermineDataValidationAttributes();
        }

        public string Name { get; private set; }
        public bool IsReadOnly { get; set; }
        public bool IncludeInBackup { get; set; }
        public bool IsInitOnlyProperty { get; set; }

        public TypeDefinition TypeDefinition { get; private set; }
        public PropertyDefinition PropertyDefinition { get; private set; }

        public object DefaultValue { get; private set; }

        public FieldDefinition BackingFieldDefinition { get; set; }
        public MethodReference ChangeCallbackReference { get; private set; }

        public bool HasDataValidationAttributes { get; private set; }
        public bool IsDataValidationAttributesSupportedByPropertyData { get; private set; }

        private void DetermineFields()
        {
            BackingFieldDefinition = TryGetField(TypeDefinition, PropertyDefinition);
        }

        private void DetermineMethods()
        {
            var methodName = $"On{PropertyDefinition.Name}Changed";

            var declaringType = PropertyDefinition.DeclaringType;

            var callbackReferences = (from method in declaringType.Methods
                                      where method.Name == methodName
                                      select method).ToList();

            foreach (var callbackReference in callbackReferences)
            {
                if (callbackReference is not null)
                {
                    if (callbackReference.HasParameters)
                    {
                        FodyEnvironment.WriteWarning($"Method '{declaringType.FullName}.{callbackReference.Name}' matches automatic change method name but has parameters and will not be used as automatic change callback. Rename the method to remove this warning or remove parameters to use as automatic callback method.");
                        continue;
                    }

                    MethodReference finalCallbackReference = callbackReference;
                    if (declaringType.HasGenericParameters)
                    {
                        finalCallbackReference = finalCallbackReference.MakeGeneric(declaringType);
                    }

                    ChangeCallbackReference = finalCallbackReference;
                    break;
                }
            }
        }

        private void DetermineDefaultValue()
        {
            var defaultValueAttribute = PropertyDefinition.GetAttribute("System.ComponentModel.DefaultValueAttribute");
            if (defaultValueAttribute is not null)
            {
                DefaultValue = defaultValueAttribute.ConstructorArguments[0].Value;
            }
        }

        private void DetermineDataValidationAttributes()
        {
            IsDataValidationAttributesSupportedByPropertyData = _catelType.PropertyDataType.GetProperty("IsDecoratedWithValidationAttributes") is not null;
            HasDataValidationAttributes = PropertyDefinition.CustomAttributes.Any(x => x.AttributeType.ImplementsBaseType("System.ComponentModel.DataAnnotations.ValidationAttribute"));
        }

        private void DetermineIncludeInBackup()
        {
            IncludeInBackup = true;

            var excludeFromBackupAttribute = PropertyDefinition.GetAttribute("Catel.Fody.ExcludeFromBackupAttribute");
            if (excludeFromBackupAttribute is not null)
            {
                IncludeInBackup = false;

                PropertyDefinition.RemoveAttribute("Catel.Fody.ExcludeFromBackupAttribute");
            }
        }

        private static FieldDefinition TryGetField(TypeDefinition typeDefinition, PropertyDefinition property)
        {
            var propertyName = property.Name;
            var fieldsWithSameType = typeDefinition.Fields.Where(x => x.DeclaringType == typeDefinition).ToList();

            foreach (var field in fieldsWithSameType)
            {
                // AutoProp
                if (field.Name == $"<{propertyName}>k__BackingField")
                {
                    return field;
                }
            }

            foreach (var field in fieldsWithSameType)
            {
                //diffCase
                var upperPropertyName = propertyName.ToUpper();
                var fieldUpper = field.Name.ToUpper();
                if (fieldUpper == upperPropertyName)
                {
                    return field;
                }

                //underScore
                if (fieldUpper == "_" + upperPropertyName)
                {
                    return field;
                }
            }

            return GetSingleField(property);
        }

        private static FieldDefinition GetSingleField(PropertyDefinition property)
        {
            var fieldDefinition = GetSingleField(property, Code.Stfld, property.SetMethod);
            if (fieldDefinition is not null)
            {
                return fieldDefinition;
            }

            return GetSingleField(property, Code.Ldfld, property.GetMethod);
        }

        private static FieldDefinition GetSingleField(PropertyDefinition property, Code code, MethodDefinition methodDefinition)
        {
            if (methodDefinition?.Body is null)
            {
                return null;
            }

            FieldReference fieldReference = null;
            foreach (var instruction in methodDefinition.Body.Instructions)
            {
                if (instruction.OpCode.Code == code)
                {
                    //if fieldReference is not null then we are at the second one
                    if (fieldReference is not null)
                    {
                        return null;
                    }

                    if (instruction.Operand is FieldReference field)
                    {
                        if (field.DeclaringType != property.DeclaringType)
                        {
                            continue;
                        }
                        if (field.FieldType != property.PropertyType)
                        {
                            continue;
                        }
                        fieldReference = field;
                    }
                }
            }

            return fieldReference?.Resolve();
        }
    }
}
