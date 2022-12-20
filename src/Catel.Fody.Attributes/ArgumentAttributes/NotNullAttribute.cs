namespace Catel.Fody
{
    using System;

    /// <summary>
    /// Not null attribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class NotNullAttribute : Attribute
    {
    }
}