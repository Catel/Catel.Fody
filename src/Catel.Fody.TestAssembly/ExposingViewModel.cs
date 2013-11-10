// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExposingViewModel.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly
{
    using Catel.Data;
    using Catel.MVVM;

    public class ExposingModel : ModelBase
    {
        #region Properties
        [DefaultValue("Geert")]
        public string FirstName { get; set; }

        public string LastName { get; set; }
        #endregion
    }

    public class ExposingViewModel : ViewModelBase
    {
        public ExposingViewModel(ExposingModel model)
        {
            Model = model;
        }

        [Model]
        [Fody.Expose("FirstName")]
        [Fody.Expose("LastName")]
        public ExposingModel Model { get; private set; }
    }
}