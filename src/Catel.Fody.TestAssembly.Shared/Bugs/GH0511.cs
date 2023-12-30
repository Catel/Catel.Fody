namespace Catel.Fody.TestAssembly.Bugs.GH0511
{
    using System;
    using System.Diagnostics;
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
            var propertyDataManager = PropertyDataManager.Default;
            var typeInfo = propertyDataManager.GetCatelTypeInfo(GetType());

            foreach (var property in typeInfo.GetCatelProperties())
            {
                Debug.WriteLine($"* {property.Key}");
            }

            if (AppSettings.SelectedThemeName != ExpectedValue)
            {
                throw new System.Exception($"Unexpected value '{AppSettings.SelectedThemeName}' on MODEL");
            }

            if (GetValue<string>("SelectedThemeName") != ExpectedValue)
            {
                throw new System.Exception($"Unexpected value '{AppSettings.SelectedThemeName}' on VIEW MODEL");
            }
        }
    }
}
