// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatelTypeProperty.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public class CatelTypeProperty
    {
        public CatelTypeProperty(TypeDefinition typeDefinition, PropertyDefinition propertyDefinition)
        {
            TypeDefinition = typeDefinition;
            PropertyDefinition = propertyDefinition;

            DetermineFields();
            DetermineMethods();
        }

        #region Fields
        public TypeDefinition TypeDefinition { get; private set; }
        public PropertyDefinition PropertyDefinition { get; private set; }

        public FieldReference BackingFieldReference { get; set; }
        public MethodReference ChangeCallbackReference { get; set; }

        #endregion

        private void DetermineFields()
        {
             BackingFieldReference = TryGetField(TypeDefinition, PropertyDefinition);
        }

        private void DetermineMethods()
        {
            string methodName = string.Format("On{0}Changed", PropertyDefinition.Name);

            ChangeCallbackReference = (from method in PropertyDefinition.DeclaringType.Methods
                                       where method.Name == methodName
                                       select method).FirstOrDefault();
        }

        private static FieldDefinition TryGetField(TypeDefinition typeDefinition, PropertyDefinition property)
        {
            var propertyName = property.Name;
            var fieldsWithSameType = typeDefinition.Fields.Where(x => x.DeclaringType == typeDefinition).ToList();
            foreach (var field in fieldsWithSameType)
            {
                //AutoProp
                if (field.Name == string.Format("<{0}>k__BackingField", propertyName))
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
                    var field = instruction.Operand as FieldReference;
                    if (field != null)
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