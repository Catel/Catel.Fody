namespace Catel.Fody
{
    using System;
    using System.Xml.Linq;

    public static class XmlExtensions
    {
        public static void ReadBool(this XElement config, string nodeName, Action<bool> setter)
        {
            var attribute = config.Attribute(nodeName);
            if (attribute is not null)
            {
                if (bool.TryParse(attribute.Value, out var value))
                {
                    setter(value);
                }
                else
                {
                    throw new WeavingException($"Could not parse '{nodeName}' from '{attribute.Value}'.");
                }
            }
        }

        public static void ReadEnum<TEnum>(this XElement config, string nodeName, Action<TEnum> setter)
            where TEnum : struct
        {
            var attribute = config.Attribute(nodeName);
            if (attribute is not null)
            {
                if (Enum.TryParse<TEnum>(attribute.Value, out var value))
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
