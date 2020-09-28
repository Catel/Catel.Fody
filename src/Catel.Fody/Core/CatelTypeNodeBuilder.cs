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
        private readonly List<TypeDefinition> _allClasses;
        private readonly MsCoreReferenceFinder _msCoreReferenceFinder;

        public List<CatelType> CatelTypes { get; private set; }

        public CatelTypeNodeBuilder(List<TypeDefinition> allTypes, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            CatelTypes = new List<CatelType>();

            _allClasses = allTypes.Where(x => x.IsClass).ToList();
            _msCoreReferenceFinder = msCoreReferenceFinder;
        }

        public void Execute()
        {
            foreach (var typeDefinition in _allClasses)
            {
                AddCatelTypeIfRequired(typeDefinition);
            }
        }

        private void AddCatelTypeIfRequired(TypeDefinition typeDefinition)
        {
            if (typeDefinition?.BaseType is null)
            {
                return;
            }

            if (typeDefinition.IsDecoratedWithAttribute("Catel.Fody.NoWeavingAttribute"))
            {
                FodyEnvironment.WriteDebug($"\t{typeDefinition.FullName} is decorated with the NoWeaving attribute, type will be ignored.");

                typeDefinition.RemoveAttribute("Catel.Fody.NoWeavingAttribute");
                return;
            }

            if (!typeDefinition.ImplementsCatelModel())
            {
                return;
            }

            var typeNode = new CatelType(typeDefinition, _msCoreReferenceFinder);
            if (typeNode.Ignore || CatelTypes.Contains(typeNode))
            {
                return;
            }

            CatelTypes.Add(typeNode);
        }
    }
}
