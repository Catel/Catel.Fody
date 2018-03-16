// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MaximumAttribute.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody
{
    using System;

    /// <summary>
    /// Max value attribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class MaximumAttribute : Attribute
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MaximumAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public MaximumAttribute(int value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaximumAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public MaximumAttribute(long value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaximumAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public MaximumAttribute(double value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaximumAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public MaximumAttribute(float value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaximumAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public MaximumAttribute(string value)
        {
        }
        #endregion
    }
}