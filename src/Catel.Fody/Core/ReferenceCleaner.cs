// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReferenceCleaner.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody
{
    using System.Linq;

    public class ReferenceCleaner
    {
        private readonly ModuleWeaver _moduleWeaver;
        public ReferenceCleaner(ModuleWeaver moduleWeaver)
        {
            _moduleWeaver = moduleWeaver;
        }

        public void Execute()
        {
            var catelFodyAttributesReference = _moduleWeaver.ModuleDefinition.AssemblyReferences.FirstOrDefault(x => string.Equals(x.Name, "Catel.Fody.Attributes"));
            if (catelFodyAttributesReference is not null)
            {
                _moduleWeaver.WriteInfo("\tRemoving reference to 'Catel.Fody.Attributes', it is no longer required.");
                _moduleWeaver.ModuleDefinition.AssemblyReferences.Remove(catelFodyAttributesReference);
            }
        }
    }
}
