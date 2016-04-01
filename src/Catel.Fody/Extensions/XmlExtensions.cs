// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlExtensions.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using System;
    using System.Xml.Linq;

    public static class XmlExtensions
    {
        public static void ReadBool(this XElement config, string nodeName, Action<bool> setter)
        {
            var attribute = config.Attribute(nodeName);
            if (attribute != null)
            {
                bool value;
                if (bool.TryParse(attribute.Value, out value))
                {
                    setter(value);
                }
                else
                {
                    throw new WeavingException($"Could not parse '{nodeName}' from '{attribute.Value}'.");
                }
            }
        }
    }
}