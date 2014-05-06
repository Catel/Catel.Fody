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
            GenerateXmlSchemas = false;

            if (config == null)
            {
                return;
            }

            config.ReadBool("GenerateXmlSchemas", value => GenerateXmlSchemas = value);
        }

        public bool GenerateXmlSchemas { get; private set; }
    }
}