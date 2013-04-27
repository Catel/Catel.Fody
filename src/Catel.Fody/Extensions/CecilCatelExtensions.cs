// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CecilCatelExtensions.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Catel.Fody
{
    using System.Collections.Generic;

    using Mono.Cecil;

    public static class CecilCatelExtensions
    {
        private static readonly Dictionary<string, bool> _typeReferencesImplementingDataObjectBase = new Dictionary<string, bool>();
        private static readonly Dictionary<string, bool> _typeReferencesImplementingModelBase = new Dictionary<string, bool>();

        public static bool ImplementsCatelModel(this TypeReference typeReference)
        {
            if (typeReference == null)
            {
                return false;
            }

            if (ImplementsModelBase(typeReference))
            {
                return true;
            }

            if (ImplementsDataObjectBase(typeReference))
            {
                return true;
            }

            return false;
        }

        public static bool ImplementsDataObjectBase(this TypeReference typeReference)
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

            implementsDataObjectBase = ImplementsBaseType(typeReference, "Catel.Data.DataObjectBase");
            _typeReferencesImplementingDataObjectBase[fullName] = implementsDataObjectBase;
            return implementsDataObjectBase;
        }

        public static bool ImplementsModelBase(this TypeReference typeReference)
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

            implementsModelBase = ImplementsBaseType(typeReference, "Catel.Data.ModelBase");
            _typeReferencesImplementingModelBase[fullName] = implementsModelBase;
            return implementsModelBase;
        }

        public static bool ImplementsBaseType(this TypeReference typeReference, string typeName)
        {
            if (typeReference == null)
            {
                return false;
            }

            TypeDefinition typeDefinition;
            if (typeReference.IsDefinition)
            {
                typeDefinition = (TypeDefinition)typeReference;
            }
            else
            {
                typeDefinition = typeReference.ResolveType();
            }

            if (DerivesFromType(typeDefinition, typeName))
            {
                return true;
            }

            return ImplementsBaseType(typeDefinition.BaseType, typeName);
        }

        public static bool DerivesFromType(this TypeDefinition typeDefinition, string typeName)
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