// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultArgumentMethodCallWeaveBase.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Argument
{
    using System.Linq;

    using Mono.Cecil;

    public class IsOfTypeArgumentMethodCallWeave : IsOfTypeOrImplementsInterfaceArgumentBase
    {
        #region Methods
      

        protected override void SelectMethod(TypeDefinition argumentTypeDefinition, out MethodDefinition selectedMethod)
        {
            selectedMethod = argumentTypeDefinition.Methods.FirstOrDefault(definition => definition.Name == "IsOfType" && definition.Parameters.Count == 3);
        }
        #endregion
    }
}