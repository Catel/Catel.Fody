// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MemberMapping.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using Mono.Cecil;

    public class MemberMapping
    {
        public MemberMapping(FieldDefinition fieldDefinition, PropertyDefinition propertyDefinition)
        {
            FieldDefinition = fieldDefinition;
            PropertyDefinition = propertyDefinition;
        }

        #region Fields
        public FieldDefinition FieldDefinition { get; private set; }
        public PropertyDefinition PropertyDefinition { get; private set; }
        #endregion
    }
}