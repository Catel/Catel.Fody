namespace Catel.Fody
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Match attribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class MatchAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MatchAttribute"/> class.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="regexOptions">The regex options.</param>
        public MatchAttribute(string pattern, RegexOptions regexOptions = RegexOptions.None)
        {
        }
    }
}