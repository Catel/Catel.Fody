// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeGenTypeCleaner.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody
{
    using System.Collections.Generic;
    using System.Linq;

    public class CodeGenTypeCleaner
    {
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;

        public CodeGenTypeCleaner(CatelTypeNodeBuilder catelTypeNodeBuilder)
        {
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
        }

        private void Process(List<CatelTypeNode> notifyNodes)
        {
            foreach (var node in notifyNodes.ToList())
            {
                var customAttributes = node.TypeDefinition.CustomAttributes;
                if (customAttributes.ContainsAttribute("System.Runtime.CompilerServices.CompilerGeneratedAttribute"))
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