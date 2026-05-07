namespace Catel.Fody.TestAssembly;

using Catel.Data;
using Catel.MVVM;

/// <summary>
/// Tests that [NoWeaving] on a parameterized OnXChanged method suppresses the weaver warning.
/// </summary>
public class NoWeavingChangeCallbackWithParametersViewModel : ViewModelBase
{
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
