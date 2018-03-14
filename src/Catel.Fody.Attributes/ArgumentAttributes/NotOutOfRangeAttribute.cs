// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotNullAttribute.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using System;

    /// <summary>
    /// Not out of range attribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class NotOutOfRangeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotOutOfRangeAttribute"/> class.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        public NotOutOfRangeAttribute(int minValue, int maxValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotOutOfRangeAttribute"/> class.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        public NotOutOfRangeAttribute(long minValue, long maxValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotOutOfRangeAttribute"/> class.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        public NotOutOfRangeAttribute(double minValue, double maxValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotOutOfRangeAttribute"/> class.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        public NotOutOfRangeAttribute(float minValue, float maxValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotOutOfRangeAttribute"/> class.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        public NotOutOfRangeAttribute(string minValue, string maxValue)
        {
        }
    }
}