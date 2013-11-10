// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WarningChecker.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.AutoProperties
{
    using System.Collections.Generic;
    using System.Linq;

    public class WarningChecker
    {
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;

        public WarningChecker(CatelTypeNodeBuilder catelTypeNodeBuilder)
        {
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
        }

        private void Process(List<CatelType> catelTypes)
        {
            foreach (var catelType in catelTypes)
            {
                foreach (var propertyData in catelType.Properties.ToList())
                {
                    var warning = CheckForWarning(propertyData);
                    if (warning != null)
                    {
                        FodyEnvironment.LogInfo(string.Format("\t{0} {1} property will be ignored.", propertyData.PropertyDefinition.GetName(), warning));
                        catelType.Properties.Remove(propertyData);
                    }
                }
            }
        }

        public string CheckForWarning(CatelTypeProperty propertyData)
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
            if ((propertyData.BackingFieldDefinition == null) && (propertyDefinition.GetMethod == null))
            {
                return "Property has no field set logic or it contains multiple sets and the names cannot be mapped to a property.";
            }
            return null;
        }

        public void Execute()
        {
            Process(_catelTypeNodeBuilder.CatelTypes);
        }
    }
}