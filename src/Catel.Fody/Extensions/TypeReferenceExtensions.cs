namespace Catel.Fody
{
    using Mono.Cecil;
    using System.Linq;

    public static class TypeReferenceExtensions
    {
        #region Methods
        public static bool IsAssignableFrom(this TypeReference target, TypeReference type)
        {
            target = type.Module.Import(target).Resolve();

            var typeDefinition = type.Resolve();

            while (typeDefinition != null)
            {
                if (typeDefinition.Equals(target))
                {
                    return true;
                }

                if (typeDefinition.Interfaces.Any(x => x.InterfaceType.Equals(target)))
                {
                    return true;
                }

                typeDefinition = typeDefinition.BaseType?.Resolve();
            }

            return false;
        }
        #endregion
    }
}