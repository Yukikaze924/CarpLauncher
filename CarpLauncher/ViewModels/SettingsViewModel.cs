using CarpLauncher.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage;

namespace CarpLauncher.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalSettingsService _localSettingsService;

    [ObservableProperty]
    private List<string> _appThemes = new()
    {
        "Default", "Light", "Dark"
    };
    [ObservableProperty]
    private string? _appTheme;
    partial void OnAppThemeChanged(string value)
    {
        if (value is null) return;

        switch (value)
        {
            case "Default":
                _themeSelectorService.SetThemeAsync(ElementTheme.Default);
                break;
            case "Light":
                _themeSelectorService.SetThemeAsync(ElementTheme.Light);
                break;
            case "Dark":
                _themeSelectorService.SetThemeAsync(ElementTheme.Dark);
                break;
        }
    }


    [ObservableProperty]
    private string? _javaExecutablePath;
    partial void OnJavaExecutablePathChanged(string? value)
    {
        if (value is null) return;

        _localSettingsService.SaveSettingAsync("JavaExecutablePath", value);
    }
    [RelayCommand]
    private async Task ChooseJavaExecutablePathAsync()
    {
        // Create a folder picker
        FolderPicker openPicker = new FolderPicker();

        // See the sample code below for how to make the window accessible from the App class.
        var window = App.MainWindow;

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        // Initialize the folder picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your folder picker
        openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
        openPicker.FileTypeFilter.Add("*");

        // Open the picker for the user to pick a folder
        StorageFolder folder = await openPicker.PickSingleFolderAsync();
        if (folder != null)
        {
            //StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
            JavaExecutablePath = folder.Path;
        }
        else
        {
            return;
        }

    }


    public SettingsViewModel(IThemeSelectorService themeSelectorService, ILocalSettingsService localSettingsService)
    {
        _themeSelectorService = themeSelectorService;
        _localSettingsService = localSettingsService;

        Task.Run(InitializeAsync);
    }

    private async Task InitializeAsync()
    {
        AppTheme = _themeSelectorService.Theme.ToString();
        JavaExecutablePath = await _localSettingsService.ReadSettingAsync<string>("JavaExecutablePath");
    }
}
