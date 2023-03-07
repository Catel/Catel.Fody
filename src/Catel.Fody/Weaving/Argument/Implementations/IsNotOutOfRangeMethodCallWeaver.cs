namespace Catel.Fody.Weaving.Argument
{
    using System.Linq;

    using Mono.Cecil;

    public sealed class IsNotOutOfRangeMethodCallWeaver : BoundariesCheckRelatedArgumentMethodCallWeaverBase
    {
        #region Methods

        protected override void SelectMethod(TypeDefinition argumentTypeDefinition, TypeReference typeToCheck, out MethodDefinition selectedMethod)
        {
            selectedMethod = argumentTypeDefinition.Methods.FirstOrDefault(definition => definition.Name == "IsNotOutOfRange" && definition.HasGenericParameters && definition.Parameters.Count == 4 && definition.Parameters[0].ParameterType.FullName == "System.String");
        }
        #endregion
    }
}