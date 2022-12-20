namespace Catel.Fody
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Not match attribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class NotMatchAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotMatchAttribute"/> class.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="regexOptions">The regex options.</param>
        public NotMatchAttribute(string pattern, RegexOptions regexOptions = RegexOptions.None)
        {
        }
    }
}