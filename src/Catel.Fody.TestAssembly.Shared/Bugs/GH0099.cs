namespace Catel.Fody.TestAssembly.Bugs.GH0099;

using System;
using Catel.MVVM;

#if CATEL_7_OR_HIGHER
public class TestViewModel : FeaturedViewModelBase
#else
public class TestViewModel : ViewModelBase
#endif
{
#if CATEL_7_OR_HIGHER
    public TestViewModel(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {

    }
#endif

    [Model]
    [Expose("ID")]
    public Model Model { get; set; }
}

public class Model : BaseModel<Model>
{

}

public class BaseModel<T> : EntityBaseWithID<int, T>
    where T : BaseModel<T>
{

}


public class EntityBaseWithID<T, S>
    where S : EntityBaseWithID<T, S>
{
    public T ID { get; set; }
}
