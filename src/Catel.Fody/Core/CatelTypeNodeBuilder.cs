// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatelTypeNodeBuilder.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;

    public class CatelTypeNodeBuilder
    {
        #region Fields
        private readonly List<TypeDefinition> _allClasses;

        public List<CatelType> CatelTypes { get; private set; }

        #endregion

        #region Constructors
        public CatelTypeNodeBuilder(List<TypeDefinition> allTypes)
        {
            CatelTypes = new List<CatelType>();

            _allClasses = allTypes.Where(x => x.IsClass).ToList();
        }
        #endregion

        #region Methods
        public void Execute()
        {
            foreach (var typeDefinition in _allClasses)
            {
                AddCatelTypeIfRequired(typeDefinition);
            }
        }

        private void AddCatelTypeIfRequired(TypeDefinition typeDefinition)
        {
            if (typeDefinition is null)
            {
                return;
            }

            if (typeDefinition.BaseType is null)
            {
                return;
            }

            if (typeDefinition.IsDecoratedWithAttribute("Catel.Fody.NoWeavingAttribute"))
            {
                FodyEnvironment.LogDebug($"\t{typeDefinition.FullName} is decorated with the NoWeaving attribute, type will be ignored.");

                typeDefinition.RemoveAttribute("Catel.Fody.NoWeavingAttribute");
                return;
            }

            if (!typeDefinition.ImplementsCatelModel())
            {
                return;
            }

            var typeNode = new CatelType(typeDefinition);
            if (typeNode.Ignore || CatelTypes.Contains(typeNode))
            {
                return;
            }

            CatelTypes.Add(typeNode);
        }
        #endregion
    }
}
