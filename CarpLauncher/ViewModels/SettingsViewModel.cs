using CarpLauncher.Contracts.Services;
using CarpLauncher.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using ProjBobcat.Class.Helper;
using ProjBobcat.DefaultComponent.Launch.GameCore;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

namespace CarpLauncher.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalSettingsService _localSettingsService;
    private readonly DefaultGameCore core = Core.Core.GetGameCore();

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
    private string _backgroundImageUrl;
    [RelayCommand]
    private async Task ChooseBackgroundImageUrlAsync()
    {
        // Create a file picker
        var openPicker = new FileOpenPicker();

        // See the sample code below for how to make the window accessible from the App class.
        var window = App.MainWindow;

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        // Initialize the file picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your file picker
        openPicker.ViewMode = PickerViewMode.Thumbnail;
        openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        openPicker.FileTypeFilter.Add(".jpg");
        openPicker.FileTypeFilter.Add(".jpeg");
        openPicker.FileTypeFilter.Add(".png");

        // Open the picker for the user to pick a file
        var file = await openPicker.PickSingleFileAsync();

        if (file != null)
        {
            BackgroundImageUrl = file.Path;
            await _localSettingsService.SaveSettingAsync<string>("BackgroundImageUrl", file.Path);
        }
        else
        {
            return;
        }
    }
    [RelayCommand]
    private async void ResetBackgroundImageUrl()
    {
        BackgroundImageUrl = "/";
        await _localSettingsService.SaveSettingAsync<string>("BackgroundImageUrl", "/");
    }


    [ObservableProperty]
    private string _gameRootPath;
    [RelayCommand]
    private async Task ChooseGameRootPathAsync()
    {
        // Create a folder picker
        var openPicker = new FolderPicker();

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
            //JavaFoundInSystem.Add(folder.Path);
            //JavaExecutablePath = folder.Path;
            GameRootPath = folder.Path;
            await _localSettingsService.SaveSettingAsync("GameRootPath", folder.Path);
        }
        else
        {
            return;
        }

    }


    [ObservableProperty]
    private string? _javaExecutablePath;
    [ObservableProperty]
    private ObservableCollection<string> _javaFoundInSystem = [];
    partial void OnJavaExecutablePathChanged(string? value)
    {
        if (value is null) return;

        _localSettingsService.SaveSettingAsync("JavaExecutablePath", value);
    }
    [RelayCommand]
    private async Task SearchForJavaInSystem()
    {
        var javas = await SystemInfoHelper.FindJava(false).ToListAsync();
        if (javas.Count == 0)
        {
            await DialogHelper.ShowRegularContentDialogAsync
                ("Error", "No Java found in system!");
            return;
        }
        if (javas.Contains(JavaExecutablePath))
        {
            await DialogHelper.ShowRegularContentDialogAsync
                ("Error", "Java already existed!");
            return;
        }

        JavaFoundInSystem = new ObservableCollection<string>(javas)
        {
            JavaExecutablePath
        };
        JavaExecutablePath = javas[0];
    }
    [RelayCommand]
    private async Task ChooseJavaExecutablePath()
    {
        // Create a folder picker
        FolderPicker openPicker = new Windows.Storage.Pickers.FolderPicker();

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
            var path = $@"{folder.Path}\bin\javaw.exe";

            if (JavaFoundInSystem.Contains(path))
            {
                await DialogHelper.ShowRegularContentDialogAsync("Error", "Java already existed!");
                return;
            }

            JavaFoundInSystem.Add(path);
            JavaExecutablePath = path;
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

    public async Task InitializeAsync()
    {
        AppTheme = _themeSelectorService.Theme.ToString();
        GameRootPath = core.RootPath;
        BackgroundImageUrl = await _localSettingsService.ReadSettingAsync<string>("BackgroundImageUrl");
        var javaPath = await _localSettingsService.ReadSettingAsync<string>("JavaExecutablePath");
        JavaFoundInSystem.Add(javaPath);
        JavaExecutablePath = javaPath;
        // 只有以上的任务可以及时的给ObservableProperty赋值，ui显示正常
        //foreach (string java in await SystemInfoHelper.FindJava(false).ToListAsync())
        //{
        //    if (java != javaPath) JavaFoundInSystem.Add(java);
        //};
        // 以下的任务无法赋值（但是没有出现异常）
    }
}
