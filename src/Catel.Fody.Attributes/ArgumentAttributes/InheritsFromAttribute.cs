namespace Catel.Fody
{
    using System;

    /// <summary>
    /// Inherits from attribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class InheritsFromAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InheritsFromAttribute"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public InheritsFromAttribute(Type type)
        {
        }
    }
}
