// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReferenceCleaner.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Mono.Cecil;

    public class ReferenceCleaner
    {
        private readonly ModuleWeaver _moduleWeaver;
        private readonly IEnumerable<AssemblyNameReference> _initialReferences;

        public ReferenceCleaner(ModuleWeaver moduleWeaver, IEnumerable<AssemblyNameReference> initialReferences)
        {
            _moduleWeaver = moduleWeaver;
            _initialReferences = initialReferences;
        }

        public void Execute()
        {
            // Remove wrong assemblies
            var currentAssemblyReferences = _moduleWeaver.ModuleDefinition.AssemblyReferences.ToList();
            foreach (var reference in currentAssemblyReferences)
            {
                if (!_initialReferences.Contains(reference))
                {
                    _moduleWeaver.LogInfo(string.Format("\tRemoving reference to '{0}', it was added by Fody but not required.", reference.FullName));
                    _moduleWeaver.ModuleDefinition.AssemblyReferences.Remove(reference);
                }
            }

            // Add replaced assemblies
            foreach (var reference in _initialReferences)
            {
                if (!_moduleWeaver.ModuleDefinition.AssemblyReferences.Contains(reference))
                {
                    _moduleWeaver.LogInfo(string.Format("\tAdding reference to '{0}', it was replaced by Fody but fixing it back.", reference.FullName));
                    _moduleWeaver.ModuleDefinition.AssemblyReferences.Add(reference);
                }
            }

            var catelFodyAttributesReference = _moduleWeaver.ModuleDefinition.AssemblyReferences.FirstOrDefault(x => string.Equals(x.Name, "Catel.Fody.Attributes"));
            if (catelFodyAttributesReference != null)
            {
                _moduleWeaver.LogInfo("\tRemoving reference to 'Catel.Fody.Attributes'.");
                _moduleWeaver.ModuleDefinition.AssemblyReferences.Remove(catelFodyAttributesReference);
            }
        }
    }
}