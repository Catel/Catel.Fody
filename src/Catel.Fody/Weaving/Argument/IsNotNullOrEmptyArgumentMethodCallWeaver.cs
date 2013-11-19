// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IsNotNullOrEmptyArgumentMethodCallWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Argument
{
    using System.Linq;

    using Mono.Cecil;

    public sealed class IsNotNullOrEmptyArgumentMethodCallWeaver : DefaultArgumentMethodCallWeaverBase
    {
        protected override void SelectMethod(TypeDefinition argumentTypeDefinition, ParameterDefinition parameter, out MethodDefinition selectedMethod)
        {
            selectedMethod = argumentTypeDefinition.Methods.FirstOrDefault(definition => definition.Name == "IsNotNullOrEmpty" && definition.Parameters.Count == 2);
        }
    }
}