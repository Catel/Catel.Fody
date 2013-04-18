// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatelModelBaseFinder.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody
{
    using System.Collections.Generic;
    using Mono.Cecil;

    public class CatelModelBaseFinder
    {
        private readonly TypeResolver _typeResolver;

        private readonly Dictionary<string, bool> _typeReferencesImplementingDataObjectBase;
        private readonly Dictionary<string, bool> _typeReferencesImplementingModelBase;

        public CatelModelBaseFinder(TypeResolver typeResolver)
        {
            _typeResolver = typeResolver;
            _typeReferencesImplementingDataObjectBase = new Dictionary<string, bool>();
            _typeReferencesImplementingModelBase = new Dictionary<string, bool>();
        }

        public bool HierarchyImplementsDataObjectBase(TypeReference typeReference)
        {
            if (typeReference == null)
            {
                return false;
            }

            bool implementsDataObjectBase;
            var fullName = typeReference.FullName;
            if (_typeReferencesImplementingDataObjectBase.TryGetValue(fullName, out implementsDataObjectBase))
            {
                return implementsDataObjectBase;
            }

            implementsDataObjectBase = HierarchyImplementsBaseType(typeReference, "Catel.Data.DataObjectBase");
            _typeReferencesImplementingDataObjectBase[fullName] = implementsDataObjectBase;
            return implementsDataObjectBase;
        }

        public bool HierachyImplementsModelBase(TypeReference typeReference)
        {
            if (typeReference == null)
            {
                return false;
            }

            bool implementsModelBase;
            var fullName = typeReference.FullName;
            if (_typeReferencesImplementingDataObjectBase.TryGetValue(fullName, out implementsModelBase))
            {
                return implementsModelBase;
            }

            implementsModelBase = HierarchyImplementsBaseType(typeReference, "Catel.Data.ModelBase");
            _typeReferencesImplementingModelBase[fullName] = implementsModelBase;
            return implementsModelBase;
        }

        private bool HierarchyImplementsBaseType(TypeReference typeReference, string typeName)
        {
            if (typeReference == null)
            {
                return false;
            }

            TypeDefinition typeDefinition;
            if (typeReference.IsDefinition)
            {
                typeDefinition = (TypeDefinition) typeReference;
            }
            else
            {
                typeDefinition = _typeResolver.Resolve(typeReference);
            }

            if (DerivesFromType(typeDefinition, typeName))
            {
                return true;
            }

            return HierarchyImplementsBaseType(typeDefinition.BaseType, typeName);
        }

        public static bool DerivesFromType(TypeDefinition typeDefinition, string typeName)
        {
            if (typeDefinition == null)
            {
                return false;
            }

            var baseType = typeDefinition.BaseType;
            if (baseType == null)
            {
                return false;
            }

            if (baseType.FullName.Contains(typeName))
            {
                return true;
            }

            return false;
        }
    }
}