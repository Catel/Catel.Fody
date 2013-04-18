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
        private readonly TypeResolver _typeResolver;
        private readonly List<TypeDefinition> _types;

        public PropertyWeaverService(ModuleWeaver moduleWeaver, CatelTypeNodeBuilder catelTypeNodeBuilder, TypeResolver typeResolver, 
            List<TypeDefinition> types)
        {
            _moduleWeaver = moduleWeaver;
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
            _typeResolver = typeResolver;
            _types = types;
        }

        public void Execute()
        {
            new DoNotNotifyTypeCleaner(_catelTypeNodeBuilder).Execute();
            new CodeGenTypeCleaner(_catelTypeNodeBuilder).Execute();
            var methodGenerifier = new MethodGenerifier(_moduleWeaver);
            new CatelPropertyMethodsFinder(methodGenerifier, _catelTypeNodeBuilder, _typeResolver).Execute();

            new AllPropertiesFinder(_catelTypeNodeBuilder).Execute();
            new MappingFinder(_catelTypeNodeBuilder).Execute();
            new PropertyDataWalker(_catelTypeNodeBuilder).Execute();
            new WarningChecker(_catelTypeNodeBuilder, _moduleWeaver).Execute();

            new CatelTypeProcessor(_catelTypeNodeBuilder, _moduleWeaver).Execute();

            new AttributeCleaner(_types).Execute();
        }
    }
}