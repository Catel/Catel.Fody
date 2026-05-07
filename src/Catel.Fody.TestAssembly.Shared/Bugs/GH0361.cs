namespace Catel.Fody.TestAssembly.Bugs.GH0361;

using System;
using Catel.MVVM;

#if CATEL_7_OR_HIGHER
public class MyDerivedViewModel : FeaturedViewModelBase
#else
public class MyDerivedViewModel : ViewModelBase
#endif
{
#if CATEL_7_OR_HIGHER
    public MyDerivedViewModel(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {

    }
#endif
    [Model]
    public TestModel Model { get; set; }

    [ViewModelToModel]
    public object Property { get; set; }

    public void SetValue(string s)
    {

    }
}

public class TestModel
{
    public object Property { get; set; }
}
