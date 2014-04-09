// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModuleWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Catel.Fody
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml.Linq;

    using Catel.Fody.Services;

    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public class ModuleWeaver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleWeaver"/> class.
        /// </summary>
        /// <remarks>
        /// The class must contain an empty constructor.
        /// </remarks>
        public ModuleWeaver()
        {
            // Init logging delegates to make testing easier
            LogInfo = s => { };
            LogWarning = s => { };
            LogError = s => { };
        }

        /// <summary>
        /// Gets or sets the configuration element. Contains the full element from <c>FodyWeavers.xml</c>.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        public XElement Config { get; set; }

        /// <summary>
        /// Gets or sets the log info delegate.
        /// </summary>
        public Action<string> LogInfo { get; set; }

        public Action<string> LogWarning { get; set; }

        public Action<string, SequencePoint> LogWarningPoint { get; set; }

        public Action<string> LogError { get; set; }

        public Action<string, SequencePoint> LogErrorPoint { get; set; }

        /// <summary>
        /// Gets or sets the assembly resolver. Contains a  <seealso cref="Mono.Cecil.IAssemblyResolver"/> for resolving dependencies.
        /// </summary>
        /// <value>
        /// The assembly resolver.
        /// </value>
        public IAssemblyResolver AssemblyResolver { get; set; }

        /// <summary>
        /// Gets or sets the module definition. Contains the Cecil representation of the assembly being built.
        /// </summary>
        /// <value>
        /// The module definition.
        /// </value>
        public ModuleDefinition ModuleDefinition { get; set; }

        public void Execute()
        {
            try
            {
                InitializeEnvironment();

                // 1st step: set up the basics
                var msCoreReferenceFinder = new MsCoreReferenceFinder(this, ModuleDefinition.AssemblyResolver);
                msCoreReferenceFinder.Execute();

                // Note: nested types not supported because we only list actual types (thus not nested)
                var types = ModuleDefinition.GetTypes().Where(x => x.IsClass && x.BaseType != null).ToList();

                var typeNodeBuilder = new CatelTypeNodeBuilder(types);
                typeNodeBuilder.Execute();

                // Remove any code generated types from the list of types to process
                var codeGenTypeCleaner = new CodeGenTypeCleaner(typeNodeBuilder);
                codeGenTypeCleaner.Execute();

                // 2nd step: Auto property weaving
                var propertyWeaverService = new AutoPropertiesWeaverService(typeNodeBuilder);
                propertyWeaverService.Execute();

                // 3rd step: Exposed properties weaving
                var exposedPropertiesWeaverService = new ExposedPropertiesWeaverService(typeNodeBuilder);
                exposedPropertiesWeaverService.Execute();

                // 4th step: Argument weaving
                var argumentWeaverService = new ArgumentWeaverService(types);
                argumentWeaverService.Execute();

                // 5th step: Xml schema weaving
                var xmlSchemasWeaverService = new XmlSchemasWeaverService(msCoreReferenceFinder, typeNodeBuilder);
                xmlSchemasWeaverService.Execute();

                // Last step: clean up
                var referenceCleaner = new ReferenceCleaner(this);
                referenceCleaner.Execute();
            }
            catch (Exception ex)
            {
                LogError(ex.Message);

#if DEBUG
                Debugger.Launch();
#endif
            }
        }

        private void InitializeEnvironment()
        {
            FodyEnvironment.ModuleDefinition = ModuleDefinition;
            FodyEnvironment.AssemblyResolver = AssemblyResolver;

            FodyEnvironment.Config = Config;
            FodyEnvironment.LogInfo = LogInfo;
            FodyEnvironment.LogWarning = LogWarning;
            FodyEnvironment.LogWarningPoint = LogWarningPoint;
            FodyEnvironment.LogError = LogError;
            FodyEnvironment.LogErrorPoint = LogErrorPoint;

            var assemblyResolver = ModuleDefinition.AssemblyResolver;

            try
            {
                FodyEnvironment.IsCatelCoreAvailable = assemblyResolver.Resolve("Catel.Core") != null;
            }
            catch (Exception)
            {
                LogError("Catel.Core is not references, cannot weave without a Catel.Core reference");
            }

            try
            {
                FodyEnvironment.IsCatelMvvmAvailable = assemblyResolver.Resolve("Catel.MVVM") != null;
            }
            catch (Exception)
            {
                LogInfo("Catel.MVVM is not referenced, skipping Catel.MVVM specific functionality");
            }
        }
    }
}