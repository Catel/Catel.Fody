// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentChecksClass.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly
{
    public class ArgumentChecksClass
    {
        public void CheckForNull([NotNull] object myObject)
        {
        }
    }
}