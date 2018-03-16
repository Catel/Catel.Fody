// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InherithsFromAttribute.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="InheritsFromAttribute"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public InheritsFromAttribute(Type type)
        {
        }
        #endregion
    }
}