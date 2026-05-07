namespace Catel.Fody.TestAssembly;

using System;
using MVVM;

public class GH0012 : ViewModelBase
{
#if CATEL_7_OR_HIGHER
    public GH0012(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {

    }
#endif

    public void a(object o)
    {
        Argument.IsNotNull(() => o);
    }
}
