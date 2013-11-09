// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatelTypeProcessor.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Properties
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Rocks;

    public class CatelTypeProcessor
    {
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;
        private readonly ModuleWeaver _moduleWeaver;

        public CatelTypeProcessor(CatelTypeNodeBuilder catelTypeNodeBuilder, ModuleWeaver moduleWeaver)
        {
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
            _moduleWeaver = moduleWeaver;
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

                _moduleWeaver.LogInfo("\t" + catelType.TypeDefinition.FullName);

                foreach (var propertyData in catelType.Properties)
                {
                    if (AlreadyContainsCallToMember(propertyData.PropertyDefinition, catelType.GetValueInvoker.Name) ||
                        AlreadyContainsCallToMember(propertyData.PropertyDefinition, catelType.SetValueInvoker.Name))
                    {
                        _moduleWeaver.LogInfo(string.Format("\t{0} Already has GetValue and/or SetValue functionality. Property will be ignored.", propertyData.PropertyDefinition.GetName()));
                        continue;
                    }

                    var body = propertyData.PropertyDefinition.SetMethod.Body;

                    body.SimplifyMacros();

                    var propertyWeaver = new CatelPropertyWeaver(_moduleWeaver, propertyData, catelType);
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