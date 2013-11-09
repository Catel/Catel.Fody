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
            var referenceToRemove = _moduleWeaver.ModuleDefinition.AssemblyReferences.FirstOrDefault(x => x.Name == "Catel.Fody.Attributes");
            if (referenceToRemove == null)
            {
                _moduleWeaver.LogInfo("\tNo reference to 'Catel.Fody.Attributes' found. References not modified.");
                return;
            }

            _moduleWeaver.ModuleDefinition.AssemblyReferences.Remove(referenceToRemove);
            _moduleWeaver.LogInfo("\tRemoving reference to 'Catel.Fody.Attributes'.");
        }
    }
}