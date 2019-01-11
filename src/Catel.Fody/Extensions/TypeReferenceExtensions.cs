namespace Catel.Fody
{
    using Mono.Cecil;
    using System.Collections.Generic;
    using System.Linq;

    public static class TypeReferenceExtensions
    {
        #region Methods
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
        #endregion
    }
}
