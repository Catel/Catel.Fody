// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultValueAttribute.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using System;

    // Note: disabled after discussion: https://catelproject.atlassian.net/browse/CTL-244 

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [Obsolete("Use System.ComponentModel.DefaultValueAttribute instead", true)]
    public class DefaultValueAttribute : Attribute
    {
        public DefaultValueAttribute(object defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public object DefaultValue { get; private set; }
    }
}