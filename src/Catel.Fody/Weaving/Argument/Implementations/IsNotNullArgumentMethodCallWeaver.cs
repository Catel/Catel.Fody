namespace Catel.Fody.Weaving.Argument
{
    using System;
    using System.Linq;

    using Mono.Cecil;

    public sealed class IsNotNullArgumentMethodCallWeaver : DefaultArgumentMethodCallWeaverBase
    {
        protected override void SelectMethod(TypeDefinition argumentTypeDefinition, TypeReference typeToCheck, out MethodDefinition selectedMethod)
        {
            var isValueType = typeToCheck.IsValueType;
            if (isValueType)
            {
                throw new Exception($"'{typeToCheck.FullName}' is a value type or struct, you cannot check for null on value types");
            }

            selectedMethod = argumentTypeDefinition.Methods.FirstOrDefault(definition => definition.Name == "IsNotNull" && definition.Parameters.Count == 2 && string.Equals(definition.Parameters[1].ParameterType.FullName, typeToCheck.FullName));
            if (selectedMethod is null)
            {
                selectedMethod = argumentTypeDefinition.Methods.FirstOrDefault(definition => definition.Name == "IsNotNull" && definition.Parameters.Count == 2);
            }
        }
    }
}
