// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatelTypeProcessor.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.AutoProperties
{
    using System.Collections.Generic;
    using Mono.Cecil.Rocks;

    public class AutoPropertiesWeaver
    {
        private readonly Configuration _configuration;
        private readonly ModuleWeaver _moduleWeaver;
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;
        private readonly MsCoreReferenceFinder _msCoreReferenceFinder;

        public AutoPropertiesWeaver(Configuration configuration, ModuleWeaver moduleWeaver,
            CatelTypeNodeBuilder catelTypeNodeBuilder, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            _configuration = configuration;
            _moduleWeaver = moduleWeaver;
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
            _msCoreReferenceFinder = msCoreReferenceFinder;
        }

        public void Execute()
        {
            Process(_catelTypeNodeBuilder.CatelTypes);
        }

        private void Process(List<CatelType> catelTypes)
        {
            foreach (var catelType in catelTypes)
            {
                FodyEnvironment.LogDebug($"\tExecuting '{GetType().Name}' for '{catelType.TypeDefinition.FullName}'");

                foreach (var propertyData in catelType.Properties)
                {
                    var body = propertyData.PropertyDefinition.SetMethod.Body;

                    body.SimplifyMacros();

                    switch (catelType.Type)
                    {
                        case CatelTypeType.ViewModel:
                        case CatelTypeType.Model:
                            var modelBasePropertyWeaver = new ModelBasePropertyWeaver(catelType, propertyData, _moduleWeaver, _msCoreReferenceFinder);
                            modelBasePropertyWeaver.Execute();
                            break;

                        case CatelTypeType.ObservableObject:
                            var observableObjectPropertyWeaver = new ObservableObjectPropertyWeaver(catelType, propertyData, _moduleWeaver, _msCoreReferenceFinder);
                            observableObjectPropertyWeaver.Execute();
                            break;

                        default:
                            break;
                    }

                    body.InitLocals = true;
                    body.OptimizeMacros();
                }

                if (_configuration.WeaveCalculatedProperties)
                {
                    var onPropertyChangedWeaver = new OnPropertyChangedWeaver(catelType, _msCoreReferenceFinder);
                    onPropertyChangedWeaver.Execute();
                }
            }
        }
    }
}
