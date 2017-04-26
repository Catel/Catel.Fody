// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GH0008.cs" company="Catel development team">
//   Copyright (c) 2008 - 2017 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly
{
    using Data;
    using MVVM;
    using Services;

    public class GH0008 : ViewModelBase
    {
        private readonly IProcessService _processService;
        private readonly IValidationContext _injectedValidationContext;

        public GH0008(IProcessService processService)
        {
            Argument.IsNotNull(() => processService);

            _processService = processService;

            ExpandAll = new Command(OnExpandAllExecute);
            CollapseAll = new Command(OnCollapseAllExecute);
            Copy = new Command(OnCopyExecute, OnCopyCanExecute);
            Open = new Command(OnOpenExecute);

            InvalidateCommandsOnPropertyChanged = true;
        }

        public GH0008(ValidationContext validationContext, IProcessService processService)
            : this(processService)
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

        public GH0008_Expected(IProcessService processService)
        {
            ShowErrors = true;
            ShowWarnings = true;

            Argument.IsNotNull("processService", processService);

            _processService = processService;

            ExpandAll = new Command(OnExpandAllExecute);
            CollapseAll = new Command(OnCollapseAllExecute);
            Copy = new Command(OnCopyExecute, OnCopyCanExecute);
            Open = new Command(OnOpenExecute);

            InvalidateCommandsOnPropertyChanged = true;
        }

        public GH0008_Expected(ValidationContext validationContext, IProcessService processService)
            : this(processService)
        {
            _injectedValidationContext = validationContext;
        }

        public bool IsExpandedAllOnStartup
        {
            get { return GetValue<bool>(IsExpandedAllOnStartupProperty); }
            set { SetValue(IsExpandedAllOnStartupProperty, value); }
        }

        public static readonly PropertyData IsExpandedAllOnStartupProperty = RegisterProperty("IsExpandedAllOnStartup", typeof(bool), false);


        public IValidationContext ValidationContext
        {
            get { return GetValue<IValidationContext>(ValidationContextProperty); }
            set { SetValue(ValidationContextProperty, value); }
        }

        public static readonly PropertyData ValidationContextProperty = RegisterProperty("ValidationContext", typeof(IValidationContext), null);


        public bool ShowErrors
        {
            get { return GetValue<bool>(ShowErrorsProperty); }
            set { SetValue(ShowErrorsProperty, value); }
        }

        public static readonly PropertyData ShowErrorsProperty = RegisterProperty("ShowErrors", typeof(bool), true);


        public bool ShowWarnings
        {
            get { return GetValue<bool>(ShowWarningsProperty); }
            set { SetValue(ShowWarningsProperty, value); }
        }

        public static readonly PropertyData ShowWarningsProperty = RegisterProperty("ShowWarnings", typeof(bool), true);


        public bool IsExpanded
        {
            get { return GetValue<bool>(IsExpandedProperty); }
            private set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly PropertyData IsExpandedProperty = RegisterProperty("IsExpanded", typeof(bool), false);


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
}