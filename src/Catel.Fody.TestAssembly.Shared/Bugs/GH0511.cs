namespace Catel.Fody.TestAssembly.Bugs.GH0511
{
    using Catel.Data;
    using Catel.MVVM;

    public class AppSettingsModel : ModelBase
    {
        public string SelectedThemeName { get; set; }
    }

    public class AppSettingsViewModel : ViewModelBase
    {
        public AppSettingsViewModel(AppSettingsModel appSettings)
        {
            AppSettings = appSettings;
        }

        public string ExpectedValue { get; set; }

        [Model]
        [Expose("SelectedThemeName")]
        public AppSettingsModel AppSettings { get; set; }

        private void OnSelectedThemeNameChanged()
        {
            if (AppSettings.SelectedThemeName != ExpectedValue)
            {
                throw new System.Exception($"Unexpected value '{AppSettings.SelectedThemeName}'");
            }
        }
    }
}
