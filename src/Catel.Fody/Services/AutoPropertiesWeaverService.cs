// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyWeaverService.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Services
{
    using Weaving.AutoProperties;

    public class AutoPropertiesWeaverService
    {
        private readonly Configuration _configuration;
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;
        private readonly MsCoreReferenceFinder _msCoreReferenceFinder;

        public AutoPropertiesWeaverService(Configuration configuration, CatelTypeNodeBuilder catelTypeNodeBuilder, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            _configuration = configuration;
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
            _msCoreReferenceFinder = msCoreReferenceFinder;
        }

        public void Execute()
        {
            var warningChecker = new AutoPropertiesWarningChecker(_catelTypeNodeBuilder);
            warningChecker.Execute();

            var weaver = new AutoPropertiesWeaver(_configuration, _catelTypeNodeBuilder, _msCoreReferenceFinder);
            weaver.Execute();
        }
    }
}