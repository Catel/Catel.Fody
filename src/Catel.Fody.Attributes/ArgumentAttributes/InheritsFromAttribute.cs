// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InherithsFromAttribute.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class InheritsFromAttribute : Attribute
    {
        #region Constructors
        public InheritsFromAttribute(Type type)
        {
        }
        #endregion
    }
}