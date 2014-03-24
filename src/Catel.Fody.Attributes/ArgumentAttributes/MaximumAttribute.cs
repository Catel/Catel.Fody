// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MaximumAttribute.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class MaximumAttribute : Attribute
    {
        #region Constructors
        public MaximumAttribute(int value)
        {
        }

        public MaximumAttribute(long value)
        {
        }

        public MaximumAttribute(double value)
        {
        }

        public MaximumAttribute(float value)
        {
        }

        public MaximumAttribute(string value)
        {
        }
        #endregion
    }
}