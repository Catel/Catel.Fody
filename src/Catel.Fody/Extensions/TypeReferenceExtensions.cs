namespace Catel.Fody
{
    using Mono.Cecil;

    public static class TypeReferenceExtensions
    {
        #region Methods
        public static bool IsAssignableFrom(this TypeReference target, TypeReference type)
        {
            target = type.Module.Import(target).Resolve();

            for (var typeDefinition = type.Resolve(); !typeDefinition.Equals(target) && !typeDefinition.Interfaces.Contains(target); typeDefinition = typeDefinition.BaseType.Resolve())
            {
                if (typeDefinition.BaseType == null)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}