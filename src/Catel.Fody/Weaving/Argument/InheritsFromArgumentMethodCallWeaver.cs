// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultArgumentMethodCallWeaveBase.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Argument
{
    using System.Linq;

    using Mono.Cecil;

    public class InheritsFromArgumentMethodCallWeaver : TypeCheckRelatedArgumentMethodCallWeaverBase
    {
        #region Methods
     
        protected override void SelectMethod(TypeDefinition argumentTypeDefinition, TypeReference typeToCheck, out MethodDefinition selectedMethod)
        {
            selectedMethod = argumentTypeDefinition.Methods.FirstOrDefault(definition => definition.Name == "InheritsFrom" && definition.Parameters.Count == 3 && ((typeToCheck.FullName == "System.Type" && definition.Parameters[1].ParameterType.FullName == "System.Type") || (typeToCheck.FullName != "System.Type" && definition.Parameters[1].ParameterType.FullName == "System.Object")));
        }
        #endregion
    }
}