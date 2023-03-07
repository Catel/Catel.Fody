namespace Catel.Fody
{
    using System;

    /// <summary>
    /// No weaving attribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method |AttributeTargets.Class, AllowMultiple = false)]
    public class NoWeavingAttribute : Attribute
    {
    }
}