// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IsMatchArgumentMethodCallWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Argument
{
    using System.Linq;

    using Mono.Cecil;

    public sealed class IsNotMatchArgumentMethodCallWeaver : RegexRelatedArgumentMethodCallWeaverBase
    {
        #region Methods
        protected override void SelectMethod(TypeDefinition argumentTypeDefinition, TypeReference typeToCheck, out MethodDefinition selectedMethod)
        {
            selectedMethod = argumentTypeDefinition.Methods.FirstOrDefault(definition => definition.Name == "IsNotMatch" && definition.Parameters.Count == 4);
        }
        #endregion
    }
}