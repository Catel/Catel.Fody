namespace Catel.Fody
{
    using System;

    /// <summary>
    /// Implements interface attribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class ImplementsInterfaceAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImplementsInterfaceAttribute"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public ImplementsInterfaceAttribute(Type type)
        {
        }
    }
}