// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentWeaverService.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Services
{
    using System;
    using System.Collections.Generic;
    using Weaving.Argument;
    using Mono.Cecil;

    public class ArgumentWeaverService
    {
        private readonly List<TypeDefinition> _allTypes;

        #region Constructors
        public ArgumentWeaverService(List<TypeDefinition> allTypes)
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
                    var weaver = new ArgumentWeaver(type);
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