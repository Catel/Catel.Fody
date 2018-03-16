// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NoWeavingAttribute.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using System;

    /// <summary>
    /// No weaving attribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public class NoWeavingAttribute : Attribute
    {
    }
}