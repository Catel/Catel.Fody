// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WarningChecker.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.AutoProperties
{
    using System.Collections.Generic;
    using System.Linq;

    public class AutoPropertiesWarningChecker
    {
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;

        public AutoPropertiesWarningChecker(CatelTypeNodeBuilder catelTypeNodeBuilder)
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
                        FodyEnvironment.WriteDebug($"\t{propertyData.PropertyDefinition.GetName()} {warning} property will be ignored.");
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

            if ((propertyData.BackingFieldDefinition is null) && (propertyDefinition.GetMethod is null))
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
