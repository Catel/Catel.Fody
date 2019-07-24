namespace Catel.Fody
{
    using Mono.Cecil;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public static class TypeReferenceExtensions
    {
        public static TypeReference ResolveGenericPropertyType(this TypeReference typeReference, PropertyReference propertyReference)
        {
            if (!propertyReference.PropertyType.IsGenericParameter)
            {
                return propertyReference.PropertyType;
            }

            // Build up hierarchy until the class that actually defines the property
            var hierarchy = typeReference.Resolve().GetHierarchy(x => x.Properties.Any(y => y.Name == propertyReference.Name));
            var genericParameter = (GenericParameter)propertyReference.PropertyType;

            var finalTypeReference = ResolveGenericParameter(hierarchy, genericParameter);
            if (finalTypeReference is null)
            {
                // Keep generic
                finalTypeReference = propertyReference.PropertyType;
            }

            return finalTypeReference;
        }

        public static IEnumerable<TypeDefinition> Traverse(this TypeDefinition typeDefinition)
        {
            var baseTypes = new List<TypeDefinition>();

            // Interfaces
            baseTypes.AddRange(typeDefinition.Interfaces.Select(x => x.InterfaceType.Resolve()));

            // Base class
            var baseType = typeDefinition.BaseType;
            if (baseType != null)
            {
                baseTypes.Add(baseType.Resolve());
            }

            // Step 1: return current base types
            foreach (var x in baseTypes)
            {
                yield return x;
            }

            // Step 2: recurse
            foreach (var x in baseTypes)
            {
                foreach (var y in Traverse(x))
                {
                    yield return y;
                }
            }
        }

        public static List<TypeWithSelfReference> GetHierarchy(this TypeDefinition typeDefinition, Func<TypeDefinition, bool> breakCondition = null)
        {
            var hierarchy = new List<TypeWithSelfReference>();

            foreach (var definition in typeDefinition.Traverse())
            {
                hierarchy.Add(new TypeWithSelfReference(definition, null));

                if (breakCondition != null && breakCondition(definition))
                {
                    break;
                }
            }

            hierarchy.Reverse();

            for (var i = 0; i < hierarchy.Count; i++)
            {
                var baseType = typeDefinition.BaseType;
                if (i < hierarchy.Count - 1)
                {
                    // Use base from hierarchy
                    baseType = hierarchy[i + 1].Type.BaseType;
                }

                hierarchy[i] = new TypeWithSelfReference(hierarchy[i].Type, baseType);
            }

            return hierarchy.ToList();
            //return hierarchy.Take(hierarchy.Count - 1).ToList();
        }

        public static TypeReference ResolveGenericParameter(IEnumerable<TypeWithSelfReference> hierarchy, GenericParameter parameter)
        {
            foreach (var (type, reference) in hierarchy)
            {
                foreach (var genericParameter in type.GenericParameters)
                {
                    if (genericParameter != parameter)
                    {
                        continue;
                    }

                    var nextArgument = ((GenericInstanceType)reference).GenericArguments[genericParameter.Position];
                    if (!(nextArgument is GenericParameter nextParameter))
                    {
                        return nextArgument;
                    }

                    parameter = nextParameter;

                    break;
                }
            }

            return null;
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

    [DebuggerDisplay("{Type} => {Reference}")]
    public class TypeWithSelfReference
    {
        public TypeWithSelfReference(TypeDefinition type, TypeReference reference)
        {
            Type = type;
            Reference = reference;
        }

        public TypeDefinition Type { get; }

        public TypeReference Reference { get; }

        public void Deconstruct(out TypeDefinition type, out TypeReference derived)
        {
            type = Type;
            derived = Reference;
        }
    }
}
