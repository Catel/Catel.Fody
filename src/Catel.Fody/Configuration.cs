// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Configuration.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


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

            if (config == null)
            {
                return;
            }

            config.ReadBool("WeaveProperties", value => WeaveProperties = value);
            config.ReadBool("WeaveExposedProperties", value => WeaveExposedProperties = value);
            config.ReadBool("WeaveCalculatedProperties", value => WeaveCalculatedProperties = value);
            config.ReadBool("WeaveArguments", value => WeaveArguments = value);
            config.ReadBool("WeaveLogging", value => WeaveLogging = value);
            config.ReadBool("GenerateXmlSchemas", value => GenerateXmlSchemas = value);
        }

        public bool WeaveProperties { get; private set; }

        public bool WeaveExposedProperties { get; private set; }

        public bool WeaveCalculatedProperties { get; private set; }

        public bool WeaveArguments { get; private set; }

        public bool WeaveLogging { get; private set; }

        public bool GenerateXmlSchemas { get; private set; }
    }
}