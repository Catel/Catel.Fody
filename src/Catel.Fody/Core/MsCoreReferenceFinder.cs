// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MsCoreReferenceFinder.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;

    public class MsCoreReferenceFinder
    {
        public const string CompilerGeneratedAttributeTypeName = "System.Runtime.CompilerServices.CompilerGeneratedAttribute";
        public const string GeneratedCodeAttributeTypeName = "System.CodeDom.Compiler.GeneratedCodeAttribute";
        public const string DebuggerNonUserCodeAttributeTypeName = "System.Diagnostics.DebuggerNonUserCodeAttribute";

        private readonly ModuleWeaver _moduleWeaver;
        private readonly IAssemblyResolver _assemblyResolver;

        private readonly IDictionary<string, TypeReference> _typeReferences = new Dictionary<string, TypeReference>();

        // Types
        public TypeReference XmlQualifiedName;
        public TypeReference XmlSchemaSet;

        public TypeReference GeneratedCodeAttribute;
        public TypeReference CompilerGeneratedAttribute;
        public TypeReference DebuggerNonUserCodeAttribute;

        public MsCoreReferenceFinder(ModuleWeaver moduleWeaver, IAssemblyResolver assemblyResolver)
        {
            _moduleWeaver = moduleWeaver;
            _assemblyResolver = assemblyResolver;
        }

        public void Execute()
        {
            var msCoreLibDefinition = _assemblyResolver.Resolve("mscorlib");
            var msCoreTypes = msCoreLibDefinition.MainModule.Types;

            var objectDefinition = msCoreTypes.FirstOrDefault(x => string.Equals(x.Name, "Object"));
            if (objectDefinition == null)
            {
                return;
            }

            var xmlDefinition = _assemblyResolver.Resolve("System.Xml");
            var xmlTypes = xmlDefinition.MainModule.Types;

            XmlQualifiedName = (from t in xmlTypes
                                where string.Equals(t.FullName, "System.Xml.XmlQualifiedName")
                                select t).FirstOrDefault();

            XmlSchemaSet = (from t in xmlTypes
                            where string.Equals(t.FullName, "System.Xml.Schema.XmlSchemaSet")
                            select t).FirstOrDefault();

            GeneratedCodeAttribute = GetCoreTypeReference(GeneratedCodeAttributeTypeName);
            CompilerGeneratedAttribute = GetCoreTypeReference(CompilerGeneratedAttributeTypeName);
            DebuggerNonUserCodeAttribute = GetCoreTypeReference(DebuggerNonUserCodeAttributeTypeName);
        }

        public TypeReference GetCoreTypeReference(string typeName)
        {
            if (!_typeReferences.ContainsKey(typeName))
            {
                var types = GetTypes();
                _typeReferences[typeName] = types.FirstOrDefault(x => string.Equals(x.Name, typeName) || string.Equals(x.FullName, typeName));
            }

            return _typeReferences[typeName];
        }

        private IEnumerable<TypeReference> GetTypes()
        {
            var msCoreLibDefinition = _assemblyResolver.Resolve("mscorlib");
            var msCoreTypes = msCoreLibDefinition.MainModule.Types.Cast<TypeReference>().ToList();

            var objectDefinition = msCoreTypes.FirstOrDefault(x => string.Equals(x.Name, "Object"));
            if (objectDefinition == null)
            {
                msCoreTypes.AddRange(GetWinRtTypes());
            }
            else
            {
                msCoreTypes.AddRange(GetDotNetTypes());
            }

            return msCoreTypes;
        }

        private IEnumerable<TypeReference> GetDotNetTypes()
        {
            var systemDefinition = _assemblyResolver.Resolve("System");
            var systemTypes = systemDefinition.MainModule.Types;

            return systemTypes;
        }

        private IEnumerable<TypeReference> GetWinRtTypes()
        {
            var systemRuntime = _assemblyResolver.Resolve("System.Runtime");
            var systemRuntimeTypes = systemRuntime.MainModule.Types;

            return systemRuntimeTypes;
        }
    }
}