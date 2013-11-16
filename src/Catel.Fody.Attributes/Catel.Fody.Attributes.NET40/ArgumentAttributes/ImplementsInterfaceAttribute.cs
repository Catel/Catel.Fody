// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatchAttribute.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class ImplementsInterfaceAttribute : Attribute
    {
        public ImplementsInterfaceAttribute(Type type)
        {
        }
    }
}