// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CTL908.cs" company="Catel development team">
//   Copyright (c) 2008 - 2016 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly
{
    using Catel;

    public class CTL908
    {
        public CTL908(object obj)
        {
            Argument.IsNotNull(() => obj);
        }
    }
}