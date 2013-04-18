// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AttributeCleaner.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Properties
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Collections.Generic;

    public class AttributeCleaner
    {
        private readonly List<TypeDefinition> _allTypes;
        private readonly List<string> _propertyAttributeNames;

        public AttributeCleaner(List<TypeDefinition> allTypes)
        {
            _propertyAttributeNames = new List<string> {"PropertyChanged.DoNotNotifyAttribute", "PropertyChanged.DoNotSetChangedAttribute", "PropertyChanged.AlsoNotifyForAttribute", "PropertyChanged.DependsOnAttribute"};
            _allTypes = allTypes;
        }

        private void ProcessType(TypeDefinition type)
        {
            RemoveAttributes(type.CustomAttributes);
            foreach (var property in type.Properties)
            {
                RemoveAttributes(property.CustomAttributes);
            }
            foreach (var field in type.Fields)
            {
                RemoveAttributes(field.CustomAttributes);
            }
        }

        private void RemoveAttributes(Collection<CustomAttribute> customAttributes)
        {
            var attributes = customAttributes
                .Where(attribute => _propertyAttributeNames.Contains(attribute.Constructor.DeclaringType.FullName));

            foreach (var customAttribute in attributes.ToList())
            {
                customAttributes.Remove(customAttribute);
            }
        }

        public void Execute()
        {
            foreach (var type in _allTypes)
            {
                ProcessType(type);
            }
        }
    }
}