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
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;

        public AutoPropertiesWeaverService(CatelTypeNodeBuilder catelTypeNodeBuilder)
        {
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
        }

        public void Execute()
        {
            var warningChecker = new AutoPropertiesWarningChecker(_catelTypeNodeBuilder);
            warningChecker.Execute();

            var weaver = new AutoPropertiesWeaver(_catelTypeNodeBuilder);
            weaver.Execute();
        }
    }
}