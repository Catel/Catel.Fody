// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggingWeaverService.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Services
{
    using System;
    using System.Collections.Generic;
    using Mono.Cecil;
    using Weaving.Logging;

    public class LoggingWeaverService
    {
        #region Fields
        private readonly List<TypeDefinition> _allTypes;
        #endregion

        #region Constructors
        public LoggingWeaverService(List<TypeDefinition> allTypes)
        {
            _allTypes = allTypes;
        }
        #endregion

        #region Methods
        public void Execute()
        {
            foreach (var type in _allTypes)
            {
                try
                {
                    var weaver = new LoggingWeaver(type);
                    weaver.Execute();
                }
                catch (Exception ex)
                {
                    FodyEnvironment.LogWarning(string.Format("Failed to weave type '{0}', message is '{1}'", type.FullName, ex.Message));
                    return;
                }
            }
        }
        #endregion
    }
}