// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GH0008.cs" company="Catel development team">
//   Copyright (c) 2008 - 2017 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly
{
    using MVVM;

    public class GH0012 : ViewModelBase
    {
        public GH0012()
        {

        }

        public void a(object o)
        {
            Argument.IsNotNull(() => o);
        }
    }
}