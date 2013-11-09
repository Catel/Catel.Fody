// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExposeAttribute.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using System;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ExposeAttribute : Attribute
    {
    }
}