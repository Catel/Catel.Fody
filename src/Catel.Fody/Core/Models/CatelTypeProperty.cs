// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatelTypeProperty.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using System.Diagnostics;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    [DebuggerDisplay("{Name}")]
    public class CatelTypeProperty
    {
        public CatelTypeProperty(TypeDefinition typeDefinition, PropertyDefinition propertyDefinition)
        {
            TypeDefinition = typeDefinition;
            PropertyDefinition = propertyDefinition;
            Name = propertyDefinition.Name;

            DetermineFields();
            DetermineMethods();
            DetermineDefaultValue();
            DetermineIncludeInBackup();
        }

        #region Fields
        public string Name { get; private set; }
        public bool IsReadOnly { get; set; }
        public bool IncludeInBackup { get; set; }

        public TypeDefinition TypeDefinition { get; private set; }
        public PropertyDefinition PropertyDefinition { get; private set; }

        public object DefaultValue { get; private set; }

        public FieldDefinition BackingFieldDefinition { get; set; }
        public MethodReference ChangeCallbackReference { get; private set; }

        #endregion
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
                if (callbackReference != null)
                {
                    if (callbackReference.HasParameters)
                    {
                        FodyEnvironment.LogWarning($"Method '{declaringType.FullName}.{callbackReference.Name}' matches automatic change method name but has parameters and will not be used as automatic change callback. Rename the method to remove this warning or remove parameters to use as automatic callback method.");
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
            //var defaultValueAttribute = PropertyDefinition.GetAttribute("Catel.Fody.DefaultValueAttribute");
            var defaultValueAttribute = PropertyDefinition.GetAttribute("System.ComponentModel.DefaultValueAttribute");
            if (defaultValueAttribute != null)
            {
                DefaultValue = defaultValueAttribute.ConstructorArguments[0].Value;

                // Catel.Fody attribute style
                //var attributeValue = (CustomAttributeArgument) defaultValueAttribute.ConstructorArguments[0].Value;
                //DefaultValue = attributeValue.Value;

                // Note: do not remove since we are now using System.ComponentModel.DefaultValueAttribute after
                // the discussion at https://catelproject.atlassian.net/browse/CTL-244
                //PropertyDefinition.RemoveAttribute("Catel.Fody.DefaultValueAttribute");
            }
        }

        private void DetermineIncludeInBackup()
        {
            IncludeInBackup = true;

            var excludeFromBackupAttribute = PropertyDefinition.GetAttribute("Catel.Fody.ExcludeFromBackupAttribute");
            if (excludeFromBackupAttribute != null)
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
                //AutoProp
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
            if (fieldDefinition != null)
            {
                return fieldDefinition;
            }
            return GetSingleField(property, Code.Ldfld, property.GetMethod);
        }

        private static FieldDefinition GetSingleField(PropertyDefinition property, Code code, MethodDefinition methodDefinition)
        {
            if (methodDefinition == null)
            {
                return null;
            }
            if (methodDefinition.Body == null)
            {
                return null;
            }
            FieldReference fieldReference = null;
            foreach (var instruction in methodDefinition.Body.Instructions)
            {
                if (instruction.OpCode.Code == code)
                {
                    //if fieldReference is not null then we are at the second one
                    if (fieldReference != null)
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
            if (fieldReference != null)
            {
                return fieldReference.Resolve();
            }
            return null;
        }
    }
}
