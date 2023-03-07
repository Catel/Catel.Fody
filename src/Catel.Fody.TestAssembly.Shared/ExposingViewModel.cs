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

    public class ExposingDerivedModel : ExposingModel
    {
        public string PropertyInDerivedClass { get; set; }
    }

    public class ExposingViewModel : ViewModelBase
    {
        public ExposingViewModel(ExposingDerivedModel model)
        {
            Model = model;
        }

        [Model]
        [Expose("FirstName")]
        [Expose("MappedLastName", "LastName")]
        [Expose("ExternalTypeProperty")]
        [Expose("ReadOnlyProperty", IsReadOnly = true)]
        [Expose("PropertyInDerivedClass")]
        public ExposingDerivedModel Model { get; private set; }

        [Model]
        [Expose("Query")]
        [Expose("Items")]
        [Expose("IsOk")]
        public ISimpleModel ExternalAssemblyModel { get; private set; }
    }
}