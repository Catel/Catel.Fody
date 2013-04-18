// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AllPropertiesFinder.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Properties
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;

    public class AllPropertiesFinder
    {
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;

        public AllPropertiesFinder(CatelTypeNodeBuilder catelTypeNodeBuilder)
        {
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
        }

        private void Process(List<CatelTypeNode> notifyNodes, List<PropertyDefinition> list)
        {
            foreach (var node in notifyNodes)
            {
                var properties = node.TypeDefinition.Properties.ToList();
                properties.AddRange(list);
                node.AllProperties = properties;
                Process(node.Nodes, properties);
            }
        }

        public void Execute()
        {
            var notifyNodes = _catelTypeNodeBuilder.NotifyNodes;
            Process(notifyNodes, new List<PropertyDefinition>());
        }
    }
}