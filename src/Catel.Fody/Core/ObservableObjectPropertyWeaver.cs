namespace Catel.Fody
{
    using System;
    using Mono.Cecil;

    public class ObservableObjectPropertyWeaver : PropertyWeaverBase
    {
        public ObservableObjectPropertyWeaver(CatelType catelType, CatelTypeProperty propertyData, ModuleWeaver moduleWeaver,
            MsCoreReferenceFinder msCoreReferenceFinder)
            : base(catelType, propertyData, moduleWeaver, msCoreReferenceFinder)
        {
        }

        public void Execute(bool force = false)
        {
            var property = _propertyData.PropertyDefinition;
            if (property is null)
            {
                FodyEnvironment.LogWarning("Skipping an unknown property because it has no property definition");
                return;
            }

            if (!force && !HasBackingField(property))
            {
                FodyEnvironment.LogDebug($"\t\tSkipping '{property.Name}' because it has no backing field");
                return;
            }

            if (!IsCleanSetter(property))
            {
                FodyEnvironment.LogDebug($"\t\tSkipping '{property.Name}' because it has no clean setter (custom implementation?)");
                return;
            }

            FodyEnvironment.LogDebug("\t\t" + property.Name);

            try
            {
                // TODO: Update 

                //var fieldDefinition = AddPropertyFieldDefinition(property);
                //if (!AddPropertyRegistration(property, _propertyData))
                //{
                //    return;
                //}

                //var fieldReference = GetFieldReference(property.DeclaringType, fieldDefinition.Name, true);

                //AddGetValueCall(property, fieldReference);
                //AddSetValueCall(property, fieldReference, _propertyData.IsReadOnly);

                //RemoveBackingField(property);
            }
            catch (Exception ex)
            {
                FodyEnvironment.LogError($"\t\tFailed to handle property '{property.DeclaringType.Name}.{property.Name}'\n{ex.Message}\n{ex.StackTrace}");

#if DEBUG
                Debugger.Launch();
#endif
            }
        }

        private bool IsCleanSetter(PropertyDefinition property)
        {
            // TODO: Determine whether there is no custom code
            return true;
        }
    }
}
