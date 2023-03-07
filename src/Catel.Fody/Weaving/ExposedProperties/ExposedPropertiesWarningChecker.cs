namespace Catel.Fody.Weaving.ExposedProperties
{
    using System.Collections.Generic;

    public class ExposedPropertiesWarningChecker
    {
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;

        public ExposedPropertiesWarningChecker(CatelTypeNodeBuilder catelTypeNodeBuilder)
        {
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
        }

        private void Process(List<CatelType> catelTypes)
        {
            //foreach (var catelType in catelTypes)
            //{
            //    foreach (var propertyData in catelType.Properties.ToList())
            //    {
            //        var warning = CheckForWarning(propertyData);
            //        if (warning != null)
            //        {
            //            FodyEnvironment.WriteDebug(string.Format("\t{0} {1} property will be ignored.", propertyData.PropertyDefinition.GetName(), warning));
            //            catelType.Properties.Remove(propertyData);
            //        }
            //    }
            //}
        }

        public string CheckForWarning(CatelTypeProperty propertyData)
        {
            //var propertyDefinition = propertyData.PropertyDefinition;
            //var setMethod = propertyDefinition.SetMethod;
            //if (setMethod.Name == "set_Item" && setMethod.Parameters.Count == 2 && setMethod.Parameters[1].Name == "value")
            //{
            //    return "Property is an indexer.";
            //}
            //if (setMethod.IsAbstract)
            //{
            //    return "Property is abstract.";
            //}
            //if ((propertyData.BackingFieldDefinition is null) && (propertyDefinition.GetMethod is null))
            //{
            //    return "Property has no field set logic or it contains multiple sets and the names cannot be mapped to a property.";
            //}
            return null;
        }

        public void Execute()
        {
            Process(_catelTypeNodeBuilder.CatelTypes);
        }
    }
}
