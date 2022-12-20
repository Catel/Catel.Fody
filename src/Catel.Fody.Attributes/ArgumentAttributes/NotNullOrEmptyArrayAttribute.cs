namespace Catel.Fody
{
    using System;

    /// <summary>
    /// Not null or empty array attribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class NotNullOrEmptyArrayAttribute : Attribute
    {
    }
}