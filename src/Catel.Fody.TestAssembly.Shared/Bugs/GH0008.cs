namespace Catel.Fody.TestAssembly;

using System;
using Data;
using MVVM;
using Services;

public class GH0008 : ViewModelBase
{
    private readonly IProcessService _processService;
    private readonly IValidationContext _injectedValidationContext;

#if CATEL_7_OR_HIGHER
    public GH0008(IServiceProvider serviceProvider, IProcessService processService)
        : base(serviceProvider)
#else
    public GH0008(IProcessService processService)
#endif
    {
        Argument.IsNotNull(() => processService);

        _processService = processService;

#if CATEL_7_OR_HIGHER
        ExpandAll = new Command(serviceProvider, OnExpandAllExecute);
        CollapseAll = new Command(serviceProvider, OnCollapseAllExecute);
        Copy = new Command(serviceProvider, OnCopyExecute, OnCopyCanExecute);
        Open = new Command(serviceProvider, OnOpenExecute);
#else
        ExpandAll = new Command(OnExpandAllExecute);
        CollapseAll = new Command(OnCollapseAllExecute);
        Copy = new Command(OnCopyExecute, OnCopyCanExecute);
        Open = new Command(OnOpenExecute);
#endif

        InvalidateCommandsOnPropertyChanged = true;
    }

#if CATEL_7_OR_HIGHER
    public GH0008(ValidationContext validationContext, IServiceProvider serviceProvider,
        IProcessService processService)
        : this(serviceProvider, processService)
#else
    public GH0008(ValidationContext validationContext, IProcessService processService)
        : this(processService)
#endif
    {
        _injectedValidationContext = validationContext;
    }

    public bool IsExpandedAllOnStartup { get; set; }
    public IValidationContext ValidationContext { get; set; }
    public bool ShowErrors { get; set; } = true;
    public bool ShowWarnings { get; set; } = true;
    public bool IsExpanded { get; private set; }
    public bool IsCollapsed => !IsExpanded;

    #region Commands
    public Command ExpandAll { get; }

    private void OnExpandAllExecute()
    {
    }

    public Command CollapseAll { get; }

    private void OnCollapseAllExecute()
    {
    }

    public Command Copy { get; }

    private bool OnCopyCanExecute()
    {
        return true;
    }

    private void OnCopyExecute()
    {

    }

    public Command Open { get; }

    private void OnOpenExecute()
    {

    }
    #endregion
}

public class GH0008_Expected : ViewModelBase
{
    private readonly IProcessService _processService;
    private readonly IValidationContext _injectedValidationContext;

#if CATEL_7_OR_HIGHER
    public GH0008_Expected(IServiceProvider serviceProvider, IProcessService processService)
        : base(serviceProvider)
#else
    public GH0008_Expected(IProcessService processService)
#endif
    {
        ShowErrors = true;
        ShowWarnings = true;

        Argument.IsNotNull("processService", processService);

        _processService = processService;

#if CATEL_7_OR_HIGHER
        ExpandAll = new Command(serviceProvider, OnExpandAllExecute);
        CollapseAll = new Command(serviceProvider, OnCollapseAllExecute);
        Copy = new Command(serviceProvider, OnCopyExecute, OnCopyCanExecute);
        Open = new Command(serviceProvider, OnOpenExecute);
#else
        ExpandAll = new Command(OnExpandAllExecute);
        CollapseAll = new Command(OnCollapseAllExecute);
        Copy = new Command(OnCopyExecute, OnCopyCanExecute);
        Open = new Command(OnOpenExecute);
#endif

        InvalidateCommandsOnPropertyChanged = true;
    }

#if CATEL_7_OR_HIGHER
    public GH0008_Expected(ValidationContext validationContext, IServiceProvider serviceProvider,
        IProcessService processService)
        : this(serviceProvider, processService)
#else
    public GH0008_Expected(ValidationContext validationContext, IProcessService processService)
        : this(processService)
#endif
    {
        _injectedValidationContext = validationContext;
    }

    public bool IsExpandedAllOnStartup
    {
        get { return GetValue<bool>(IsExpandedAllOnStartupProperty); }
        set { SetValue(IsExpandedAllOnStartupProperty, value); }
    }

#if CATEL_5
    public static readonly PropertyData IsExpandedAllOnStartupProperty = RegisterProperty("IsExpandedAllOnStartup", typeof(bool), false);
#elif CATEL_6_OR_HIGHER
    public static readonly IPropertyData IsExpandedAllOnStartupProperty = RegisterProperty<bool>("IsExpandedAllOnStartup", false);
#endif

    public IValidationContext ValidationContext
    {
        get { return GetValue<IValidationContext>(ValidationContextProperty); }
        set { SetValue(ValidationContextProperty, value); }
    }

#if CATEL_5
    public static readonly PropertyData ValidationContextProperty = RegisterProperty("ValidationContext", typeof(IValidationContext), null);
#elif CATEL_6_OR_HIGHER
    public static readonly IPropertyData ValidationContextProperty = RegisterProperty<IValidationContext>("ValidationContext");
#endif

    public bool ShowErrors
    {
        get { return GetValue<bool>(ShowErrorsProperty); }
        set { SetValue(ShowErrorsProperty, value); }
    }

#if CATEL_5
    public static readonly PropertyData ShowErrorsProperty = RegisterProperty("ShowErrors", typeof(bool), true);
#elif CATEL_6_OR_HIGHER
    public static readonly IPropertyData ShowErrorsProperty = RegisterProperty<bool>("ShowErrors", true);
#endif

    public bool ShowWarnings
    {
        get { return GetValue<bool>(ShowWarningsProperty); }
        set { SetValue(ShowWarningsProperty, value); }
    }

#if CATEL_5
    public static readonly PropertyData ShowWarningsProperty = RegisterProperty("ShowWarnings", typeof(bool), true);
#elif CATEL_6_OR_HIGHER
    public static readonly IPropertyData ShowWarningsProperty = RegisterProperty<bool>("ShowWarnings", true);
#endif

    public bool IsExpanded
    {
        get { return GetValue<bool>(IsExpandedProperty); }
        private set { SetValue(IsExpandedProperty, value); }
    }

#if CATEL_5
    public static readonly PropertyData IsExpandedProperty = RegisterProperty("IsExpanded", typeof(bool), false);
#elif CATEL_6_OR_HIGHER
    public static readonly IPropertyData IsExpandedProperty = RegisterProperty<bool>("IsExpanded", false);
#endif

    public bool IsCollapsed => !IsExpanded;

    #region Commands
    public Command ExpandAll { get; }

    private void OnExpandAllExecute()
    {
    }

    public Command CollapseAll { get; }

    private void OnCollapseAllExecute()
    {
    }

    public Command Copy { get; }

    private bool OnCopyCanExecute()
    {
        return true;
    }

    private void OnCopyExecute()
    {

    }

    public Command Open { get; }

    private void OnOpenExecute()
    {

    }
    #endregion
}
