namespace Catel.Fody.TestAssembly;

using System;
using System.ComponentModel;
using Catel.Data;
using Catel.Fody.TestAssembly.Bugs.GH0511;
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

#if CATEL_7_OR_HIGHER
public class ExposingViewModel : FeaturedViewModelBase
#else
public class ExposingViewModel : ViewModelBase
#endif
{
#if CATEL_7_OR_HIGHER
    public ExposingViewModel(ExposingDerivedModel model, IServiceProvider serviceProvider)
        : base(serviceProvider)
#else
    public ExposingViewModel(ExposingDerivedModel model)
#endif
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
