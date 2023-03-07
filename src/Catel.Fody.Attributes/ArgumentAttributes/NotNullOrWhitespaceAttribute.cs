namespace Catel.Fody
{
    using System;

    /// <summary>
    /// Not null or whites space attribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class NotNullOrWhitespaceAttribute : Attribute
    {
    }
}