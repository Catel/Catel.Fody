// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Inheritance.cs" company="Catel development team">
//   Copyright (c) 2008 - 2012 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.TestAssembly
{
    using Data;

    public class BaseClass : ModelBase
    {
        public string PropertyOnBase { get; set; }
    }

    public class InheritedClass : BaseClass
    {
        public string PropertyOnInherited { get; set; }
    }
}