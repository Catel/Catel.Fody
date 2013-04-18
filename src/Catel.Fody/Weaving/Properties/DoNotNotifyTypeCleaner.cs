// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoNotNotifyTypeCleaner.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Properties
{
    using System.Collections.Generic;
    using System.Linq;

    public class DoNotNotifyTypeCleaner
    {
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;

        public DoNotNotifyTypeCleaner(CatelTypeNodeBuilder catelTypeNodeBuilder)
        {
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
        }

        private void Process(List<CatelTypeNode> notifyNodes)
        {
            foreach (var node in notifyNodes.ToList())
            {
                var containsDoNotNotifyAttribute = node.TypeDefinition.CustomAttributes.ContainsAttribute("PropertyChanged.DoNotNotifyAttribute");
                if (containsDoNotNotifyAttribute)
                {
                    notifyNodes.Remove(node);
                    continue;
                }
                Process(node.Nodes);
            }
        }

        public void Execute()
        {
            Process(_catelTypeNodeBuilder.NotifyNodes);
        }
    }
}