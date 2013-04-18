// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlSchemasWeaverService.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Services
{
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
            var xmlSchemaWeaver = new XmlSchemaWeaver(_moduleWeaver, _msCoreReferenceFinder);
            foreach (var catelTypeNode in _catelTypeNodeBuilder.Nodes)
            {
                if (!CatelVersionSupportsXmlSchemaManager(catelTypeNode))
                {
                    return;
                }

                xmlSchemaWeaver.Execute(catelTypeNode);
            }
        }

        private bool CatelVersionSupportsXmlSchemaManager(CatelTypeNode catelTypeNode)
        {
            if (!_isSupported.HasValue)
            {
                if (_msCoreReferenceFinder.XmlQualifiedName == null || _msCoreReferenceFinder.XmlSchemaSet == null)
                {
                    return false;
                }

                var xmlSchemaManager = (TypeDefinition) catelTypeNode.TypeDefinition.Module.FindType("Catel.Core", "Catel.Runtime.Serialization.XmlSchemaManager");
                _isSupported = xmlSchemaManager != null;
            }

            return _isSupported.Value;
        }
    }
}