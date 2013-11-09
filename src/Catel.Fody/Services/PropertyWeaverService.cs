// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyWeaverService.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Services
{
    using System.Collections.Generic;
    using Mono.Cecil;
    using Weaving.Properties;

    public class PropertyWeaverService
    {
        private readonly ModuleWeaver _moduleWeaver;
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;
        private readonly List<TypeDefinition> _types;

        public PropertyWeaverService(ModuleWeaver moduleWeaver, CatelTypeNodeBuilder catelTypeNodeBuilder, List<TypeDefinition> types)
        {
            _moduleWeaver = moduleWeaver;
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
            _types = types;
        }

        public void Execute()
        {
            new CodeGenTypeCleaner(_catelTypeNodeBuilder).Execute();

            new WarningChecker(_catelTypeNodeBuilder, _moduleWeaver).Execute();

            new CatelTypeProcessor(_catelTypeNodeBuilder, _moduleWeaver).Execute();
        }
    }
}