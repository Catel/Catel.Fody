// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IsMinimalMethodCallWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Weaving.Argument
{
    using System.Linq;
    using Mono.Cecil;

    public class IsMinimalMethodCallWeaver : BoundariesCheckRelatedArgumentMethodCallWeaverBase
    {
        #region Methods
        protected override void SelectMethod(TypeDefinition argumentTypeDefinition, TypeReference typeToCheck, out MethodDefinition selectedMethod)
        {
            selectedMethod = argumentTypeDefinition.Methods.FirstOrDefault(definition => definition.Name == "IsMinimal" && definition.HasGenericParameters && definition.Parameters.Count == 3 && definition.Parameters[0].ParameterType.FullName == "System.String");
        }
        #endregion
    }
}