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
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;
        private readonly MsCoreReferenceFinder _msCoreReferenceFinder;

        public AutoPropertiesWeaver(CatelTypeNodeBuilder catelTypeNodeBuilder, MsCoreReferenceFinder msCoreReferenceFinder)
        {
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

                var onPropertyChangedWeaver = new OnPropertyChangedWeaver(catelType, _msCoreReferenceFinder);
                onPropertyChangedWeaver.Execute();

                FodyEnvironment.LogDebug("\t" + catelType.TypeDefinition.FullName);

                foreach (var propertyData in catelType.Properties)
                {
                    if (AlreadyContainsCallToMember(propertyData.PropertyDefinition, catelType.GetValueInvoker.Name) ||
                        AlreadyContainsCallToMember(propertyData.PropertyDefinition, catelType.SetValueInvoker.Name))
                    {
                        FodyEnvironment.LogDebug(string.Format("\t{0} already has GetValue and/or SetValue functionality. Property will be ignored.", propertyData.PropertyDefinition.GetName()));
                        continue;
                    }

                    var body = propertyData.PropertyDefinition.SetMethod.Body;

                    body.SimplifyMacros();

                    var propertyWeaver = new CatelPropertyWeaver(catelType, propertyData, _msCoreReferenceFinder);
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