// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotMatchAttribute.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody
{
    using System;
    using System.Text.RegularExpressions;

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class NotMatchAttribute : Attribute
    {
        public NotMatchAttribute(string pattern, RegexOptions regexOptions = RegexOptions.None)
        {
        }
    }
}