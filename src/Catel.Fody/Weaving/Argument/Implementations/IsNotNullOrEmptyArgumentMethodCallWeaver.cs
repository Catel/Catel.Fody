﻿namespace Catel.Fody.Weaving.Argument
{
    using System.Linq;

    using Mono.Cecil;

    public sealed class IsNotNullOrEmptyArgumentMethodCallWeaver : DefaultArgumentMethodCallWeaverBase
    {
        protected override void SelectMethod(TypeDefinition argumentTypeDefinition, TypeReference typeToCheck, out MethodDefinition selectedMethod)
        {
            selectedMethod = argumentTypeDefinition.Methods.FirstOrDefault(definition => definition.Name == "IsNotNullOrEmpty" && definition.Parameters.Count == 2 && string.Equals(definition.Parameters[1].ParameterType.FullName, typeToCheck.FullName));
        }
    }
}