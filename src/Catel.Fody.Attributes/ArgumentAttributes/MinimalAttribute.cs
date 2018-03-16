// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MinimalAttribute.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody
{
    using System;

    /// <summary>
    /// Minimal attribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class MinimalAttribute : Attribute
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MinimalAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public MinimalAttribute(int value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinimalAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public MinimalAttribute(long value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinimalAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public MinimalAttribute(double value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinimalAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public MinimalAttribute(float value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinimalAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public MinimalAttribute(string value)
        {
        }
        #endregion
    }
}