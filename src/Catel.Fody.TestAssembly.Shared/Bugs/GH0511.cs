namespace Catel.Fody.TestAssembly.Bugs.GH0511;

using System;
using Catel.Data;
using Catel.MVVM;

public class AppSettingsModel : ModelBase
{
    public string SelectedThemeName { get; set; }
}

#if CATEL_7_OR_HIGHER
public class AppSettingsViewModel : FeaturedViewModelBase
#else
public class AppSettingsViewModel : ViewModelBase
#endif
{
#if CATEL_7_OR_HIGHER
    public AppSettingsViewModel(AppSettingsModel appSettings, IServiceProvider serviceProvider)
        : base(serviceProvider)
#else
    public AppSettingsViewModel(AppSettingsModel appSettings)
#endif
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
