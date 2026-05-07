#if !CATEL_5

namespace Catel.Fody.TestAssembly.Bugs.GH0531;

using System;
using System.Diagnostics;
using Catel.Data;
using Catel.Fody.TestAssembly.Bugs.GH0511;
using Catel.MVVM;

public class TestModel : ObservableObject
{
    private int _counter;

    public object MyProperty { get; set; }

    public int Counter { get => _counter; }

    private void OnMyPropertyChanged()
    {
        _counter++;

        Debug.WriteLine($"Property changed: {MyProperty} {_counter}");
    }
}

#if CATEL_7_OR_HIGHER
public class MyDerivedViewModel : FeaturedViewModelBase
#else
public class MyDerivedViewModel : ViewModelBase
#endif
{
#if CATEL_7_OR_HIGHER
    public MyDerivedViewModel(TestModel testModel, IServiceProvider serviceProvider)
        : base(serviceProvider)
#else
    public MyDerivedViewModel(TestModel testModel)
#endif
    {
        Model = testModel;
    }

    [Model]
    public TestModel Model { get; set; }

    [ViewModelToModel]
    public object MyProperty { get; set; }
}

#endif
