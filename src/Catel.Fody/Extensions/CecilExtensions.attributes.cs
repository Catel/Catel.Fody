// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CecilExtensions.attributes.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using System.Linq;
    using Mono.Cecil;
    using Mono.Collections.Generic;

    public static partial class CecilExtensions
    {
        public static bool IsDecoratedWithAttribute(this TypeDefinition typeDefinition, string attributeName)
        {
            return IsDecoratedWithAttribute(typeDefinition.CustomAttributes, attributeName);
        }

        public static bool IsDecoratedWithAttribute(this PropertyDefinition propertyDefinition, string attributeName)
        {
            return IsDecoratedWithAttribute(propertyDefinition.CustomAttributes, attributeName);
        }

        public static bool IsDecoratedWithAttribute(Collection<CustomAttribute> customAttributes, string attributeName)
        {
            return (from attribute in customAttributes
                    where attribute.Constructor.DeclaringType.FullName.Contains(attributeName)
                    select attribute).Any();
        }

        public static void RemoveAttribute(this TypeDefinition typeDefinition, string attributeName)
        {
            RemoveAttribute(typeDefinition.CustomAttributes, attributeName);
        }

        public static void RemoveAttribute(this PropertyDefinition propertyDefinition, string attributeName)
        {
            RemoveAttribute(propertyDefinition.CustomAttributes, attributeName);
        }

        public static void RemoveAttribute(Collection<CustomAttribute> customAttributes, string attributeName)
        {
            var attributes = (from attribute in customAttributes
                              where attribute.Constructor.DeclaringType.FullName.Contains(attributeName)
                              select attribute).ToList();

            foreach (var attribute in attributes)
            {
                customAttributes.Remove(attribute);
            }
        }
    }
}