// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlSchemasWeaverService.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Services
{
    using System;

    using Mono.Cecil;
    using Weaving.XmlSchemas;

    public class XmlSchemasWeaverService
    {
        private readonly ModuleWeaver _moduleWeaver;
        private readonly MsCoreReferenceFinder _msCoreReferenceFinder;
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;

        private bool? _isSupported;

        public XmlSchemasWeaverService(ModuleWeaver moduleWeaver, MsCoreReferenceFinder msCoreReferenceFinder, CatelTypeNodeBuilder catelTypeNodeBuilder)
        {
            _moduleWeaver = moduleWeaver;
            _msCoreReferenceFinder = msCoreReferenceFinder;
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
        }

        public void Execute()
        {
            var xmlSchemaWeaver = new XmlSchemasWeaver(_moduleWeaver, _msCoreReferenceFinder);
            foreach (var catelTypeNode in _catelTypeNodeBuilder.CatelTypes)
            {
                try
                {
                    if (!CatelVersionSupportsXmlSchemaManager(catelTypeNode))
                    {
                        return;
                    }

                    xmlSchemaWeaver.Execute(catelTypeNode);
                }
                catch (Exception)
                {
#if DEBUG
                    System.Diagnostics.Debugger.Launch();
#endif

                    var error = $"An error occurred while weaving type '{catelTypeNode.TypeDefinition.FullName}'";
                    FodyEnvironment.LogError(error);
                }
            }
        }

        private bool CatelVersionSupportsXmlSchemaManager(CatelType catelType)
        {
            if (catelType is null)
            {
                return false;
            }

            if (!_isSupported.HasValue)
            {
                if (_msCoreReferenceFinder.XmlQualifiedName is null || _msCoreReferenceFinder.XmlSchemaSet is null)
                {
                    return false;
                }

                var xmlSchemaManager = (TypeDefinition)catelType.TypeDefinition.Module.FindType("Catel.Core", "Catel.Runtime.Serialization.Xml.XmlSchemaManager");
                _isSupported = xmlSchemaManager != null;

                if (!_isSupported ?? false)
                {
                    var xmlSchemaManagerPre38 = (TypeDefinition)catelType.TypeDefinition.Module.FindType("Catel.Core", "Catel.Runtime.Serialization.XmlSchemaManager");
                    _isSupported = xmlSchemaManagerPre38 != null;
                }
            }

            return _isSupported.Value;
        }
    }
}
