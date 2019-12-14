// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModuleWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Catel.Fody
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using global::Fody;
    using Services;

    using Mono.Cecil;

    public class ModuleWeaver : BaseModuleWeaver
    {
        public ModuleWeaver()
        {
            // Init logging delegates to make testing easier
            LogDebug = s => { Debug.WriteLine(s); };
            LogInfo = s => { Debug.WriteLine(s); };
            LogWarning = s => { Debug.WriteLine(s); };
            LogWarningPoint = (s, p) => { Debug.WriteLine(s); };
            LogError = s => { Debug.WriteLine(s); };
            LogErrorPoint = (s, p) => { Debug.WriteLine(s); };
        }

        public IAssemblyResolver AssemblyResolver { get; set; }

        public override bool ShouldCleanReference => true;

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            var assemblies = new List<string>();

            // For now just return all references
            assemblies.Add("netstandard");
            assemblies.AddRange(ModuleDefinition.AssemblyReferences.Select(x => x.Name));

            return assemblies;
        }

        public override void Execute()
        {
            try
            {
#if DEBUG
                if (!Debugger.IsAttached)
                {
                    Debugger.Launch();

                    FodyEnvironment.LogDebug = CreateLoggingCallback(LogDebug);
                    FodyEnvironment.LogInfo = CreateLoggingCallback(LogInfo);
                    FodyEnvironment.LogWarning = CreateLoggingCallback(LogWarning);
                    FodyEnvironment.LogError = CreateLoggingCallback(LogError);
                }
#endif

                // First of all, set the assembly resolver
                if (AssemblyResolver is null)
                {
                    AssemblyResolver = ModuleDefinition.AssemblyResolver;
                }

                if (TypeSystem is null)
                {
                    var typeCache = new global::Fody.TypeCache(x => AssemblyResolver.Resolve(x));
                    typeCache.BuildAssembliesToScan(this);

                    TypeSystem = new global::Fody.TypeSystem(x => typeCache.FindType(x), ModuleDefinition);
                }

                // Clear cache because static members will be re-used over multiple builds over multiple systems
                CacheHelper.ClearAllCaches();

                var configuration = new Configuration(Config);

                InitializeEnvironment();

                LogInfo($"Catel.Fody v{GetType().Assembly.GetName().Version}");

                // 1st step: set up the basics
                var msCoreReferenceFinder = new MsCoreReferenceFinder(this, ModuleDefinition.AssemblyResolver);
                msCoreReferenceFinder.Execute();

                // Validate if Catel.Core is referenced
                configuration.IsRunningAgainstCatel = false;

                if (ModuleDefinition.Name.StartsWith("Catel.Core") ||
                    ModuleDefinition.Name.StartsWith("Catel.MVVM") ||
                    ModuleDefinition.Name.StartsWith("Catel.Serialization.") ||
                    ModuleDefinition.Name.StartsWith("Catel.Tests"))
                {
                    configuration.IsRunningAgainstCatel = true;

                    LogInfo("Running against Catel itself, most features will be disabled");
                }

                var catelCoreReference = AssemblyResolver.Resolve("Catel.Core");
                if (!configuration.IsRunningAgainstCatel && catelCoreReference is null)
                {
                    LogWarning("No reference to Catel.Core found, this weaver is useless without referencing Catel");
                    return;
                }

                // Note: nested types not supported because we only list actual types (thus not nested)
                var types = ModuleDefinition.GetTypes().Where(x => x.IsClass && x.BaseType != null).ToList();

                var typeNodeBuilder = new CatelTypeNodeBuilder(types);
                typeNodeBuilder.Execute();

                // Remove any code generated types from the list of types to process
                var codeGenTypeCleaner = new CodeGenTypeCleaner(typeNodeBuilder);
                codeGenTypeCleaner.Execute();

                // 2nd step: Auto property weaving
                if (!configuration.IsRunningAgainstCatel && configuration.WeaveProperties)
                {
                    FodyEnvironment.LogInfo("Weaving properties");

                    var propertyWeaverService = new AutoPropertiesWeaverService(configuration, this, typeNodeBuilder, msCoreReferenceFinder);
                    propertyWeaverService.Execute();
                }
                else
                {
                    FodyEnvironment.LogInfo("Weaving properties is disabled");
                }

                // 3rd step: Exposed properties weaving
                if (!configuration.IsRunningAgainstCatel && configuration.WeaveExposedProperties)
                {
                    FodyEnvironment.LogInfo("Weaving exposed properties");

                    var exposedPropertiesWeaverService = new ExposedPropertiesWeaverService(this, typeNodeBuilder, msCoreReferenceFinder);
                    exposedPropertiesWeaverService.Execute();
                }
                else
                {
                    FodyEnvironment.LogInfo("Weaving exposed properties is disabled");
                }

                // 4th step: Argument weaving
                if (configuration.WeaveArguments)
                {
                    FodyEnvironment.LogInfo("Weaving arguments");

                    var argumentWeaverService = new ArgumentWeaverService(types, msCoreReferenceFinder, configuration);
                    argumentWeaverService.Execute();
                }
                else
                {
                    FodyEnvironment.LogInfo("Weaving arguments is disabled");
                }

                // 5th step: Logging weaving (we will run this against Catel.Core)
                if (configuration.WeaveLogging)
                {
                    FodyEnvironment.LogInfo("Weaving logging");

                    var loggingWeaver = new LoggingWeaverService(types);
                    loggingWeaver.Execute();
                }
                else
                {
                    FodyEnvironment.LogInfo("Weaving logging is disabled");
                }

                // 6th step: Xml schema weaving
                if (!configuration.IsRunningAgainstCatel && configuration.GenerateXmlSchemas)
                {
                    FodyEnvironment.LogInfo("Weaving xml schemas");

                    var xmlSchemasWeaverService = new XmlSchemasWeaverService(this, msCoreReferenceFinder, typeNodeBuilder);
                    xmlSchemasWeaverService.Execute();
                }
                else
                {
                    FodyEnvironment.LogInfo("Weaving xml schemas is disabled");
                }

                // Validate that nothing has been left out
                var validationService = new ValidationService(this);
                validationService.Validate();

                // Last step: clean up
                var referenceCleaner = new ReferenceCleaner(this);
                referenceCleaner.Execute();
            }
            catch (Exception ex)
            {
                LogError(ex.Message);

#if DEBUG
                if (!Debugger.IsAttached)
                {
                    Debugger.Launch();
                }
#endif
            }
        }

        private static Action<string> CreateLoggingCallback(Action<string> callback)
        {
            return s =>
            {
                Trace.WriteLine(s);

                callback?.Invoke(s);
            };
        }

        private void InitializeEnvironment()
        {
            FodyEnvironment.ModuleDefinition = ModuleDefinition;
            FodyEnvironment.AssemblyResolver = AssemblyResolver;

            FodyEnvironment.Config = Config;
            FodyEnvironment.LogDebug = LogDebug;
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
                LogError("Catel.Core is not referenced, cannot weave without a Catel.Core reference");
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
