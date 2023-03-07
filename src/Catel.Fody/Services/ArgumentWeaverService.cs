namespace Catel.Fody.Services
{
    using System;
    using System.Collections.Generic;
    using Weaving.Argument;
    using Mono.Cecil;

    public class ArgumentWeaverService
    {
        private readonly List<TypeDefinition> _allTypes;
        private readonly MsCoreReferenceFinder _msCoreReferenceFinder;
        private readonly Configuration _configuration;

        #region Constructors
        public ArgumentWeaverService(List<TypeDefinition> allTypes, MsCoreReferenceFinder msCoreReferenceFinder,
            Configuration configuration)
        {
            _allTypes = allTypes;
            _msCoreReferenceFinder = msCoreReferenceFinder;
            _configuration = configuration;
        }
        #endregion

        #region Methods
        public void Execute()
        {
            foreach (var type in _allTypes)
            {
                try
                {
                    var weaver = new ArgumentWeaver(type, _msCoreReferenceFinder, _configuration);
                    weaver.Execute();
                }
                catch (Exception ex)
                {
                    FodyEnvironment.WriteWarning($"Failed to weave type '{type.FullName}', message is '{ex.Message}'");
                    return;
                }
            }
        }
        #endregion
    }
}
