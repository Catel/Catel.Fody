namespace Catel.Fody.TestAssembly;

using System;
using Catel.Fody.TestAssembly.Bugs.GH0511;
using Data;
using MVVM;

public class ViewModelBaseTest : MyViewModelBase
{

#if CATEL_7_OR_HIGHER
    public ViewModelBaseTest(IServiceProvider serviceProvider)
        : base(serviceProvider)
#else
    public ViewModelBaseTest()
#endif
    {
#if CATEL_7_OR_HIGHER
        TestCommand = new Command(serviceProvider, OnTestCommandExecute);
        TestCommandWithInterface = new Command(serviceProvider, OnTestCommandWithInterfaceExecute);
#else
        TestCommand = new Command(OnTestCommandExecute);
        TestCommandWithInterface = new Command(OnTestCommandWithInterfaceExecute);
#endif
    }

    public string Name { get; set; }

    public string FullName
    {
        get { return GetValue<string>(FullNameProperty); }
        set { SetValue(FullNameProperty, value); }
    }

#if CATEL_5
    public static readonly PropertyData FullNameProperty = RegisterProperty("FullName", typeof(string));
#elif CATEL_6_OR_HIGHER
    public static readonly IPropertyData FullNameProperty = RegisterProperty<string>("FullName");
#endif

    private void OnNameChanged()
    {

    }

    /// <summary>
    /// Gets the TestCommand command.
    /// </summary>
    public Command TestCommand { get; private set; }

    /// <summary>
    /// Method to invoke when the TestCommand command is executed.
    /// </summary>
    private void OnTestCommandExecute()
    {
        // TODO: Handle command logic here
    }

    /// <summary>
    /// Gets the TestCommandWithInterface command.
    /// </summary>
    public ICatelCommand TestCommandWithInterface { get; private set; }

    /// <summary>
    /// Method to invoke when the TestCommandWithInterface command is executed.
    /// </summary>
    private void OnTestCommandWithInterfaceExecute()
    {
        // TODO: Handle command logic here
    }
}
