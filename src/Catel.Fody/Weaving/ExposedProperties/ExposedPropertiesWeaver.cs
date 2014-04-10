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
        private readonly MsCoreReferenceFinder _msCoreReferenceFinder;
        private readonly TypeDefinition ViewModelToModelAttributeTypeDefinition;

        public ExposedPropertiesWeaver(CatelTypeNodeBuilder catelTypeNodeBuilder, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            ViewModelToModelAttributeTypeDefinition = FodyEnvironment.ModuleDefinition.FindType("Catel.MVVM", "Catel.MVVM.ViewModelToModelAttribute") as TypeDefinition;

            _catelTypeNodeBuilder = catelTypeNodeBuilder;
            _msCoreReferenceFinder = msCoreReferenceFinder;
        }

        public void Execute()
        {
            if (ViewModelToModelAttributeTypeDefinition == null)
            {
                return;
            }

            foreach (var catelType in _catelTypeNodeBuilder.CatelTypes)
            {
                if (catelType.TypeDefinition.ImplementsViewModelBase())
                {
                    ProcessType(catelType);
                }
            }
        }

        private void ProcessType(CatelType catelType)
        {
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

            bool isReadOnly = false;
            var isReadOnlyProperty = (from property in exposeAttribute.Properties
                                      where string.Equals(property.Name, "IsReadOnly")
                                      select property).FirstOrDefault();

            if (isReadOnlyProperty.Argument.Value != null)
            {
                isReadOnly = (bool) isReadOnlyProperty.Argument.Value;
            }

            // Check property definition on model
            var modelType = modelProperty.PropertyDefinition.PropertyType;
            var modelPropertyToMap = modelType.GetProperty(modelPropertyName);
            if (modelPropertyToMap == null)
            {
                FodyEnvironment.LogError(string.Format("Exposed property '{0}' does not exist on model '{1}', make sure to set the right mapping", modelPropertyName, modelType.FullName));
                return;
            }

            var modelPropertyType = modelPropertyToMap.PropertyType;

            var viewModelPropertyDefinition = new PropertyDefinition(viewModelPropertyName, PropertyAttributes.None, FodyEnvironment.ModuleDefinition.Import(modelPropertyType));
            viewModelPropertyDefinition.DeclaringType = catelType.TypeDefinition;

            var compilerGeneratedAttribute = catelType.TypeDefinition.Module.FindType("mscorlib", "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
            viewModelPropertyDefinition.CustomAttributes.Add(new CustomAttribute(catelType.TypeDefinition.Module.Import(compilerGeneratedAttribute.Resolve().Constructor(false))));

            catelType.TypeDefinition.Properties.Add(viewModelPropertyDefinition);
            var catelTypeProperty = new CatelTypeProperty(catelType.TypeDefinition, viewModelPropertyDefinition);
            catelTypeProperty.IsReadOnly = isReadOnly;

            var catelPropertyWeaver = new CatelPropertyWeaver(catelType, catelTypeProperty, _msCoreReferenceFinder);
            catelPropertyWeaver.Execute(true);

            var stringType = _msCoreReferenceFinder.GetCoreTypeReference("String");
            var stringTypeDefinition = catelType.TypeDefinition.Module.Import(stringType);

            var attributeConstructor = catelType.TypeDefinition.Module.Import(ViewModelToModelAttributeTypeDefinition.Constructor(false));
            var viewModelToModelAttribute = new CustomAttribute(attributeConstructor);
            viewModelToModelAttribute.ConstructorArguments.Add(new CustomAttributeArgument(stringTypeDefinition, modelName));
            viewModelToModelAttribute.ConstructorArguments.Add(new CustomAttributeArgument(stringTypeDefinition, modelPropertyName));
            viewModelPropertyDefinition.CustomAttributes.Add(viewModelToModelAttribute);
        }
    }
}