// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExposingViewModel.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly
{
    using System.ComponentModel;
    using Catel.Data;
    using Catel.MVVM;

    public class ExposingModel : ModelBase
    {
        #region Properties
        [DefaultValue("Geert")]
        public string FirstName { get; set; }

        [DefaultValue("van Horrik")]
        public string LastName { get; set; }

        [DefaultValue("ReadOnly")]
        public string ReadOnlyProperty { get; set; }
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
        [Fody.Expose("MappedLastName", "LastName")]
        [Fody.Expose("ExternalTypeProperty")]
        [Fody.Expose("ReadOnlyProperty", IsReadOnly = true)]
        public ExposingModel Model { get; private set; }

        [Model]
        [Catel.Fody.Expose("Query")]
        [Catel.Fody.Expose("Items")]
        [Catel.Fody.Expose("IsOk")]
        public ISimpleModel ExternalAssemblyModel { get; private set; }
    }
}