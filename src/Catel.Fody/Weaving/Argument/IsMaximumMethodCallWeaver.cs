// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IsMaximumMethodCallWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Argument
{
    using System.Linq;

    using Mono.Cecil;

    public sealed class IsMaximumMethodCallWeaver : BoundariesCheckRelatedArgumentMethodCallWeaverBase
    {
        #region Methods

        protected override void SelectMethod(TypeDefinition argumentTypeDefinition, ParameterDefinition parameter, out MethodDefinition selectedMethod)
        {
            selectedMethod = argumentTypeDefinition.Methods.FirstOrDefault(definition => definition.Name == "IsMaximum" && definition.HasGenericParameters && definition.Parameters.Count == 3 && definition.Parameters[0].ParameterType.FullName == "System.String");
        }
        #endregion
    }
}