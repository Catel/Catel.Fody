// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MsCoreReferenceFinder.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody
{
    using System.Linq;
    using Mono.Cecil;

    public class MsCoreReferenceFinder
    {
        private readonly ModuleWeaver _moduleWeaver;
        private readonly IAssemblyResolver _assemblyResolver;

        // Types
        public TypeReference XmlQualifiedName;
        public TypeReference XmlSchemaSet;

        // Methods
        public MethodReference ObjectConstructor;

        public MsCoreReferenceFinder(ModuleWeaver moduleWeaver, IAssemblyResolver assemblyResolver)
        {
            _moduleWeaver = moduleWeaver;
            _assemblyResolver = assemblyResolver;
        }

        public void Execute()
        {
            var msCoreLibDefinition = _assemblyResolver.Resolve("mscorlib");
            var msCoreTypes = msCoreLibDefinition.MainModule.Types;

            var objectDefinition = msCoreTypes.FirstOrDefault(x => x.Name == "Object");
            if (objectDefinition == null)
            {
                ExecuteWinRT();
                return;
            }
            var module = _moduleWeaver.ModuleDefinition;
            var constructorDefinition = objectDefinition.Methods.First(x => x.IsConstructor);
            ObjectConstructor = module.Import(constructorDefinition);

            //var nullableDefinition = msCoreTypes.FirstOrDefault(x => x.Name == "Nullable");
            //NullableEqualsMethod = module.Import(nullableDefinition).Resolve().Methods.First(x => x.Name == "Equals");

            //var systemDefinition = _assemblyResolver.Resolve("System");
            //var systemTypes = systemDefinition.MainModule.Types;

            var xmlDefinition = _assemblyResolver.Resolve("System.Xml");
            var xmlTypes = xmlDefinition.MainModule.Types;

            XmlQualifiedName = (from t in xmlTypes
                                where t.FullName == "System.Xml.XmlQualifiedName"
                                select t).FirstOrDefault();

            XmlSchemaSet = (from t in xmlTypes
                            where t.FullName == "System.Xml.Schema.XmlSchemaSet"
                            select t).FirstOrDefault();
        }

        public void ExecuteWinRT()
        {
            var systemRuntime = _assemblyResolver.Resolve("System.Runtime");
            var systemRuntimeTypes = systemRuntime.MainModule.Types;

            var objectDefinition = systemRuntimeTypes.First(x => x.Name == "Object");
            var module = _moduleWeaver.ModuleDefinition;
            var constructorDefinition = objectDefinition.Methods.First(x => x.IsConstructor);
            ObjectConstructor = module.Import(constructorDefinition);

            //var nullableDefinition = systemRuntimeTypes.FirstOrDefault(x => x.Name == "Nullable");
            //NullableEqualsMethod = module.Import(nullableDefinition).Resolve().Methods.First(x => x.Name == "Equals");
        }
    }
}