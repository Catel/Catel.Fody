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
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;

        public ExposedPropertiesWeaverService(CatelTypeNodeBuilder catelTypeNodeBuilder)
        {
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
        }

        public void Execute()
        {
            var warningChecker = new ExposedPropertiesWarningChecker(_catelTypeNodeBuilder);
            warningChecker.Execute();

            var weaver = new ExposedPropertiesWeaver(_catelTypeNodeBuilder);
            weaver.Execute();
        }
    }
}