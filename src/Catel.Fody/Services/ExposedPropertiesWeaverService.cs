// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExposedPropertiesWeaverService.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Services
{
    using Catel.Fody.Weaving.ExposedProperties;

    public class ExposedPropertiesWeaverService
    {
        private readonly ModuleWeaver _moduleWeaver;
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;
        private readonly MsCoreReferenceFinder _msCoreReferenceFinder;

        public ExposedPropertiesWeaverService(ModuleWeaver moduleWeaver, CatelTypeNodeBuilder catelTypeNodeBuilder, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            _moduleWeaver = moduleWeaver;
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
            _msCoreReferenceFinder = msCoreReferenceFinder;
        }

        public void Execute()
        {
            if (!FodyEnvironment.IsCatelMvvmAvailable)
            {
                FodyEnvironment.LogInfo("Skipping weaving of exposed properties because this is an MVVM feature");
                return;
            }

            var warningChecker = new ExposedPropertiesWarningChecker(_catelTypeNodeBuilder);
            warningChecker.Execute();

            var weaver = new ExposedPropertiesWeaver(_catelTypeNodeBuilder, _moduleWeaver, _msCoreReferenceFinder);
            weaver.Execute();
        }
    }
}
