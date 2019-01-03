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
        private static readonly Dictionary<string, bool> _implementsTypeCache = CacheHelper.GetCache<Dictionary<string, bool>>("CecilCatelExtensions");

        public static bool ImplementsCatelModel(this TypeReference typeReference)
        {
            if (typeReference is null)
            {
                return false;
            }

            if (ImplementsViewModelBase(typeReference))
            {
                return true;
            }

            if (ImplementsModelBase(typeReference))
            {
                return true;
            }

            if (ImplementsObservableObject(typeReference))
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
            return ImplementsBaseType(typeReference, "Catel.Data.DataObjectBase");
        }

        public static bool ImplementsObservableObject(this TypeReference typeReference)
        {
            return ImplementsBaseType(typeReference, "Catel.Data.ObservableObject");
        }

        public static bool ImplementsModelBase(this TypeReference typeReference)
        {
            return ImplementsBaseType(typeReference, "Catel.Data.ModelBase");
        }

        public static bool ImplementsViewModelBase(this TypeReference typeReference)
        {
            return ImplementsBaseType(typeReference, "Catel.MVVM.ViewModelBase");
        }

        public static bool ImplementsBaseType(this TypeReference typeReference, string typeName)
        {
            if (typeReference == null)
            {
                return false;
            }

            string requestKey = $"{typeReference.FullName}_{typeName}";
            bool implementsModelBase;
            if (_implementsTypeCache.TryGetValue(requestKey, out implementsModelBase))
            {
                return implementsModelBase;
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
                _implementsTypeCache[requestKey] = true;
                return true;
            }

            var result =  ImplementsBaseType(typeDefinition.BaseType, typeName);
            _implementsTypeCache[requestKey] = result;
            return result;
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
