// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatelTypeProcessor.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.AutoProperties
{
    using System.Collections.Generic;
    using System.Linq;

    using Mono.Cecil;
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
                if (catelType.SetValueInvoker == null)
                {
                    continue;
                }

                if (_configuration.WeaveCalculatedProperties)
                {
                    var onPropertyChangedWeaver = new OnPropertyChangedWeaver(catelType, _msCoreReferenceFinder);
                    onPropertyChangedWeaver.Execute();
                }

                FodyEnvironment.LogDebug("\t" + catelType.TypeDefinition.FullName);

                foreach (var propertyData in catelType.Properties)
                {
                    if (AlreadyContainsCallToMember(propertyData.PropertyDefinition, catelType.GetValueInvoker.Name) ||
                        AlreadyContainsCallToMember(propertyData.PropertyDefinition, catelType.SetValueInvoker.Name))
                    {
                        FodyEnvironment.LogDebug($"\t{propertyData.PropertyDefinition.GetName()} already has GetValue and/or SetValue functionality. Property will be ignored.");
                        continue;
                    }

                    var body = propertyData.PropertyDefinition.SetMethod.Body;

                    body.SimplifyMacros();

                    var propertyWeaver = new CatelPropertyWeaver(catelType, propertyData, _moduleWeaver, _msCoreReferenceFinder);
                    propertyWeaver.Execute();

                    body.InitLocals = true;
                    body.OptimizeMacros();
                }
            }
        }

        public static bool AlreadyContainsCallToMember(PropertyDefinition propertyDefinition, string methodName)
        {
            var instructions = propertyDefinition.SetMethod.Body.Instructions;
            return instructions.Any(x =>
                                    x.OpCode.IsCall() &&
                                    x.Operand is MethodReference &&
                                    ((MethodReference) x.Operand).Name == methodName);
        }
    }
}
