// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotNullAttribute.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using System;

    /// <summary>
    /// Not null or empty array attribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class NotNullOrEmptyArrayAttribute : Attribute
    {
    }
}