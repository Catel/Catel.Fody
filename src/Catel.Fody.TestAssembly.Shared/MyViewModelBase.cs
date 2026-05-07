namespace Catel.Fody.TestAssembly;

using System;
using Catel.Fody.TestAssembly.Bugs.GH0511;

public class MyViewModelBase : MVVM.ViewModelBase
{
#if CATEL_7_OR_HIGHER
    public MyViewModelBase(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {

    }
#endif
}
