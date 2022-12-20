namespace Catel.Fody.Services
{
    using Weaving.AutoProperties;

    public class AutoPropertiesWeaverService
    {
        private readonly Configuration _configuration;
        private readonly ModuleWeaver _moduleWeaver;
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;
        private readonly MsCoreReferenceFinder _msCoreReferenceFinder;

        public AutoPropertiesWeaverService(Configuration configuration, ModuleWeaver moduleWeaver,
            CatelTypeNodeBuilder catelTypeNodeBuilder, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            _configuration = configuration;
            _moduleWeaver = moduleWeaver;
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
            _msCoreReferenceFinder = msCoreReferenceFinder;
        }

        public void Execute()
        {
            var warningChecker = new AutoPropertiesWarningChecker(_catelTypeNodeBuilder);
            warningChecker.Execute();

            var weaver = new AutoPropertiesWeaver(_configuration, _moduleWeaver, _catelTypeNodeBuilder, _msCoreReferenceFinder);
            weaver.Execute();
        }
    }
}
