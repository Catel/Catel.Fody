// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModuleWithDoubleValues.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.TestAssembly
{
    using Catel.Data;

    /// <summary>
    /// Required to reproduce https://catelproject.atlassian.net/browse/CTL-237.
    /// </summary>
    public class ModelWithDoubleValues : ModelBase
    {
        public double Top { get; set; }

        public double Left { get; set; }

        // If 3rd property uncommented the code runs without exception but then
        // Top  = 0.0
        // Left = 2.7338856697455278E-305
        // I would expect both having the default(double) value => 0.0
        public double Width { get; set; }
    }
}