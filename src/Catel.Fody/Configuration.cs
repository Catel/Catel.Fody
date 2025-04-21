namespace Catel.Fody
{
    using System.Xml.Linq;

    public class Configuration
    {
        public Configuration(XElement config)
        {
            WeaveProperties = true;
            WeaveExposedProperties = true;
            WeaveCalculatedProperties = true;
            WeaveArguments = true;
            WeaveLogging = true;
            GenerateXmlSchemas = false;
            DisableWarningsForAutoPropertyInitializers = false;
            GeneratedPropertyDataAccessibility = Accessibility.Public;

            if (config is null)
            {
                return;
            }

            config.ReadBool(nameof(WeaveProperties), value => WeaveProperties = value);
            config.ReadBool(nameof(WeaveExposedProperties), value => WeaveExposedProperties = value);
            config.ReadBool(nameof(WeaveCalculatedProperties), value => WeaveCalculatedProperties = value);
            config.ReadBool(nameof(WeaveArguments), value => WeaveArguments = value);
            config.ReadBool(nameof(WeaveLogging), value => WeaveLogging = value);
            config.ReadBool(nameof(GenerateXmlSchemas), value => GenerateXmlSchemas = value);
            config.ReadBool(nameof(DisableWarningsForAutoPropertyInitializers), value => DisableWarningsForAutoPropertyInitializers = value);

            config.ReadEnum<Accessibility>(nameof(GeneratedPropertyDataAccessibility), value => GeneratedPropertyDataAccessibility = value);
        }

        public bool IsRunningAgainstCatel { get; set; }

        public bool WeaveProperties { get; private set; }

        public bool WeaveExposedProperties { get; private set; }

        public bool WeaveCalculatedProperties { get; private set; }

        public bool WeaveArguments { get; private set; }

        public bool WeaveLogging { get; private set; }

        public bool GenerateXmlSchemas { get; private set; }

        public bool DisableWarningsForAutoPropertyInitializers { get; private set; }

        public Accessibility GeneratedPropertyDataAccessibility { get; private set; }
    }
}
