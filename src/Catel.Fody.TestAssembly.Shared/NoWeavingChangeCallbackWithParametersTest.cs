namespace Catel.Fody.TestAssembly;

using System;
using Catel.Data;
using Catel.MVVM;

/// <summary>
/// Tests that [NoWeaving] on a parameterized OnXChanged method suppresses the weaver warning.
/// </summary>
public class NoWeavingChangeCallbackWithParametersViewModel : ViewModelBase
{
#if CATEL_7_OR_HIGHER
    public NoWeavingChangeCallbackWithParametersViewModel(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {

    }
#endif

    public object SelectedItem { get; set; }

    public bool WasCallbackInvoked { get; private set; }

    [NoWeaving]
    protected virtual void OnSelectedItemChanged(object item)
    {
        WasCallbackInvoked = true;
    }

    private void OnSelectedItemChanged()
    {
        OnSelectedItemChanged(SelectedItem);
    }
}
