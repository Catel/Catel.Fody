namespace Catel.Fody
{
    using Mono.Cecil;
    using System.Collections.Generic;
    using System.Linq;

    public static class TypeReferenceExtensions
    {
        public static TypeReference ResolveGenericPropertyType(this TypeReference typeReference, PropertyReference propertyReference)
        {
            if (!propertyReference.PropertyType.IsGenericParameter)
            {
                return propertyReference.PropertyType;
            }

            // This would return "T" in "Model<T>"
            var genericParameterName = propertyReference.PropertyType.Name;

            // Important: make sure the declaring type itself isn't providing this element
            var declaredOnDeclaringTypeItself = (from x in typeReference.GenericParameters
                                                 where x.Name == genericParameterName
                                                 select x).Any();
            if (declaredOnDeclaringTypeItself)
            {
                // Property is meant to be generic
                return propertyReference.PropertyType;
            }

            var finalTypeReference = propertyReference.PropertyType;

            // Search down the line if we can find the generic parameter type
            var type = typeReference;
            var previousType = typeReference;
            while (type != null)
            {
                var genericParameter = (from x in type.Resolve().GenericParameters
                                        where x.Name == genericParameterName
                                        select x).FirstOrDefault();
                if (genericParameter is null)
                {
                    previousType = type;
                    type = type.Resolve().BaseType;
                    continue;
                }

                var parentGenericInstance = ((TypeDefinition)previousType).BaseType as GenericInstanceType;
                if (parentGenericInstance is null)
                {
                    // Unable to resolve
                    break;
                }

                // Get right right index
                finalTypeReference = parentGenericInstance.GenericArguments[genericParameter.Position];
                break;
            }

            // Note: note sure if this method supports "redefinitions" of multiple generics, but in that 
            // case this method should recursively call itself to resolve from the declaringType again in
            // case the new property != provided property, but we for now we keep this method simple

            return finalTypeReference;
        }
        public static bool IsAssignableFrom(this TypeReference target, TypeReference type)
        {
            target = type.Module.ImportReference(target).Resolve();

            var checkedInterfaces = new HashSet<InterfaceImplementation>();
            var typeDefinition = type.Resolve();

            while (typeDefinition != null)
            {
                if (typeDefinition.Equals(target))
                {
                    return true;
                }

                var interfaces = typeDefinition.Interfaces;

                foreach (var iface in interfaces)
                {
                    if (checkedInterfaces.Contains(iface))
                    {
                        continue;
                    }

                    checkedInterfaces.Add(iface);

                    if (iface.InterfaceType.Equals(target))
                    {
                        return true;
                    }
                }

                typeDefinition = typeDefinition.BaseType?.Resolve();
            }

            return false;
        }
    }
}
