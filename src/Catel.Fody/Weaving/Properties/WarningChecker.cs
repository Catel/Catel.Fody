// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WarningChecker.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Properties
{
    using System.Collections.Generic;
    using System.Linq;

    public class WarningChecker
    {
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;
        private readonly ModuleWeaver _moduleWeaver;

        public WarningChecker(CatelTypeNodeBuilder catelTypeNodeBuilder, ModuleWeaver moduleWeaver)
        {
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
            _moduleWeaver = moduleWeaver;
        }

        private void Process(List<CatelTypeNode> notifyNodes)
        {
            foreach (var node in notifyNodes)
            {
                foreach (var propertyData in node.PropertyDatas.ToList())
                {
                    var warning = CheckForWarning(propertyData);
                    if (warning != null)
                    {
                        _moduleWeaver.LogInfo(string.Format("\t{0} {1} Property will be ignored.", propertyData.PropertyDefinition.GetName(), warning));
                        node.PropertyDatas.Remove(propertyData);
                    }
                }

                Process(node.Nodes);
            }
        }

        public string CheckForWarning(PropertyData propertyData)
        {
            var propertyDefinition = propertyData.PropertyDefinition;
            var setMethod = propertyDefinition.SetMethod;
            if (setMethod.Name == "set_Item" && setMethod.Parameters.Count == 2 && setMethod.Parameters[1].Name == "value")
            {
                return "Property is an indexer.";
            }
            if (setMethod.IsAbstract)
            {
                return "Property is abstract.";
            }
            if ((propertyData.BackingFieldReference == null) && (propertyDefinition.GetMethod == null))
            {
                return "Property has no field set logic or it contains multiple sets and the names cannot be mapped to a property.";
            }
            return null;
        }

        public void Execute()
        {
            Process(_catelTypeNodeBuilder.NotifyNodes);
        }
    }
}