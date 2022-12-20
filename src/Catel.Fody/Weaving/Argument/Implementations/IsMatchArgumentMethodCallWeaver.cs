namespace Catel.Fody.Weaving.Argument
{
    using System.Linq;

    using Mono.Cecil;

    public sealed class IsMatchArgumentMethodCallWeaver : RegexRelatedArgumentMethodCallWeaverBase
    {
        #region Methods

        protected override void SelectMethod(TypeDefinition argumentTypeDefinition, TypeReference typeToCheck, out MethodDefinition selectedMethod)
        {
            selectedMethod = argumentTypeDefinition.Methods.FirstOrDefault(definition => definition.Name == "IsMatch" && definition.Parameters.Count == 4);
        }
        #endregion
    }
}