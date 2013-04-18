// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeResolver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody
{
    using System;
    using System.Collections.Generic;
    using Mono.Cecil;

    public class TypeResolver
    {
        private readonly Dictionary<string, TypeDefinition> _definitions;

        public TypeResolver()
        {
            _definitions = new Dictionary<string, TypeDefinition>();
        }

        public TypeDefinition Resolve(TypeReference reference)
        {
            TypeDefinition definition;
            if (_definitions.TryGetValue(reference.FullName, out definition))
            {
                return definition;
            }
            return _definitions[reference.FullName] = InnerResolve(reference);
        }

        private static TypeDefinition InnerResolve(TypeReference reference)
        {
            try
            {
                return reference.Resolve();
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format("Could not resolve '{0}'.", reference.FullName), exception);
            }
        }
    }
}