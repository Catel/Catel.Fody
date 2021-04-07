// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExposedPropertiesWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Weaving.ExposedProperties
{
    using System.Linq;
    using Mono.Cecil;

    public class ExposedPropertiesWeaver
    {
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;
        private readonly ModuleWeaver _moduleWeaver;
        private readonly Configuration _configuration;
        private readonly MsCoreReferenceFinder _msCoreReferenceFinder;
        private readonly TypeDefinition _viewModelToModelAttributeTypeDefinition;

        public ExposedPropertiesWeaver(CatelTypeNodeBuilder catelTypeNodeBuilder, ModuleWeaver moduleWeaver,
            Configuration configuration, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            _viewModelToModelAttributeTypeDefinition = FodyEnvironment.ModuleDefinition.FindType("Catel.MVVM", "Catel.MVVM.ViewModelToModelAttribute") as TypeDefinition;

            _catelTypeNodeBuilder = catelTypeNodeBuilder;
            _moduleWeaver = moduleWeaver;
            _configuration = configuration;
            _msCoreReferenceFinder = msCoreReferenceFinder;
        }

        public void Execute()
        {
            if (_viewModelToModelAttributeTypeDefinition is null)
            {
                return;
            }

            foreach (var catelType in _catelTypeNodeBuilder.CatelTypes)
            {
                if (catelType.Type == CatelTypeType.ViewModel)
                {
                    ProcessType(catelType);
                }
            }
        }

        private void ProcessType(CatelType catelType)
        {
            FodyEnvironment.WriteDebug($"\tExecuting '{GetType().Name}' for '{catelType.TypeDefinition.FullName}'");

            foreach (var property in catelType.Properties)
            {
                var propertyDefinition = property.PropertyDefinition;
                var exposeAttributes = propertyDefinition.GetAttributes("Catel.Fody.ExposeAttribute");
                foreach (var exposeAttribute in exposeAttributes)
                {
                    ProcessProperty(catelType, property, exposeAttribute);
                }

                propertyDefinition.RemoveAttribute("Catel.Fody.ExposeAttribute");
            }
        }

        private void ProcessProperty(CatelType catelType, CatelTypeProperty modelProperty, CustomAttribute exposeAttribute)
        {
            var modelName = modelProperty.Name;
            var viewModelPropertyName = (string)exposeAttribute.ConstructorArguments[0].Value;
            var modelPropertyName = viewModelPropertyName;
            if (exposeAttribute.ConstructorArguments.Count > 1)
            {
                modelPropertyName = (string)(exposeAttribute.ConstructorArguments[1].Value ?? viewModelPropertyName);
            }

            var isReadOnly = false;
            var isReadOnlyProperty = (from property in exposeAttribute.Properties
                                      where string.Equals(property.Name, "IsReadOnly")
                                      select property).FirstOrDefault();

            if (isReadOnlyProperty.Argument.Value is not null)
            {
                isReadOnly = (bool)isReadOnlyProperty.Argument.Value;
            }

            // Check property definition on model
            var modelType = modelProperty.PropertyDefinition.PropertyType;
            var modelPropertyToMap = modelType.GetProperty(modelPropertyName);
            if (modelPropertyToMap is null)
            {
                FodyEnvironment.WriteError($"Exposed property '{modelPropertyName}' does not exist on model '{modelType.FullName}', make sure to set the right mapping");
                return;
            }

            var modelPropertyType = modelType.ResolveGenericPropertyType(modelPropertyToMap);

            var viewModelPropertyDefinition = new PropertyDefinition(viewModelPropertyName, PropertyAttributes.None, FodyEnvironment.ModuleDefinition.ImportReference(modelPropertyType));
            viewModelPropertyDefinition.DeclaringType = catelType.TypeDefinition;

            catelType.TypeDefinition.Properties.Add(viewModelPropertyDefinition);

            viewModelPropertyDefinition.MarkAsCompilerGenerated(_msCoreReferenceFinder);

            var catelTypeProperty = new CatelTypeProperty(catelType.TypeDefinition, viewModelPropertyDefinition);
            catelTypeProperty.IsReadOnly = isReadOnly;

            var catelPropertyWeaver = new ModelBasePropertyWeaver(catelType, catelTypeProperty, _configuration, _moduleWeaver, _msCoreReferenceFinder);
            catelPropertyWeaver.Execute(true);

            var stringType = _msCoreReferenceFinder.GetCoreTypeReference("String");
            var stringTypeDefinition = catelType.TypeDefinition.Module.ImportReference(stringType);

            var attributeConstructor = catelType.TypeDefinition.Module.ImportReference(_viewModelToModelAttributeTypeDefinition.Constructor(false));
            var viewModelToModelAttribute = new CustomAttribute(attributeConstructor);
            viewModelToModelAttribute.ConstructorArguments.Add(new CustomAttributeArgument(stringTypeDefinition, modelName));
            viewModelToModelAttribute.ConstructorArguments.Add(new CustomAttributeArgument(stringTypeDefinition, modelPropertyName));
            viewModelPropertyDefinition.CustomAttributes.Add(viewModelToModelAttribute);
        }
    }
}
