// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MinimalAttribute.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class MinimalAttribute : Attribute
    {
        #region Constructors
        public MinimalAttribute(int value)
        {
        }

        public MinimalAttribute(long value)
        {
        }

        public MinimalAttribute(double value)
        {
        }

        public MinimalAttribute(float value)
        {
        }

        public MinimalAttribute(string value)
        {
        }
        #endregion
    }
}