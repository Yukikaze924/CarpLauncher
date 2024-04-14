using CarpLauncher.Contracts.Services;
using CarpLauncher.Core;
using CarpLauncher.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProjBobcat.Class.Helper;
using ProjBobcat.Class.Model;
using ProjBobcat.Class.Model.LauncherProfile;
using ProjBobcat.DefaultComponent.Authenticator;
using ProjBobcat.DefaultComponent.Launch.GameCore;

namespace CarpLauncher.ViewModels;

public partial class HomeViewModel : ObservableRecipient
{
    private readonly ILocalSettingsService _localSettingsService;
    private readonly INavigationService _navigationService;
    private DefaultGameCore core;

    [ObservableProperty]
    private string _backgroundImageUrl;

    [ObservableProperty]
    private string _currentSelectedVersion = "choose a version";
    [RelayCommand]
    private async Task LaunchMinecraft()
    {
        if (string.IsNullOrWhiteSpace(CurrentSelectedVersion)
        ||
        CurrentSelectedVersion == "choose a version")
        {
            var result = await DialogHelper.ShowRegularContentDialogAsync
            ("Error", "Choose a version before the game launch!", "Choose");

            if (result == ContentDialogResult.Primary)
            {
                _navigationService.NavigateTo(typeof(GameViewModel).FullName!);
            }
            return;
        }

        core = Core.Core.GetGameCore();

        VersionInfo? selectedGame;
        try
        {
            selectedGame = core.VersionLocator.GetGame(CurrentSelectedVersion);
            if (selectedGame is null)
            {
                throw new InvalidDataException();
            }
        }
        catch
        {
            await DialogHelper.ShowRegularContentDialogAsync
            ("Error", $"{CurrentSelectedVersion} does not existed!");
            CurrentSelectedVersion = "choose a version";
            return;
        }

        try
        {
            var isVersionIsolate = await _localSettingsService.ReadSettingAsync<bool>("IsVersionIsolate");
            var javaPath = await _localSettingsService.ReadSettingAsync<string>("JavaExecutablePath");

            var launchSettings = new LaunchSettings
            {
                Version = selectedGame.Id, // 需要启动的游戏ID
                VersionInsulation = isVersionIsolate, // 版本隔离
                GameResourcePath = core.RootPath, // 资源根目录
                GamePath = (isVersionIsolate) ? core.RootPath + GamePathHelper.GetGamePath(selectedGame.Id) : core.RootPath, // 游戏根目录，如果有版本隔离则应该改为GamePathHelper.GetGamePath(Core.RootPath, versionId)
                VersionLocator = core.VersionLocator, // 游戏定位器
                GameName = selectedGame.Name,
                GameArguments = new GameArguments // （可选）具体游戏启动参数
                {
                    AdvanceArguments = "", // GC类型
                    JavaExecutable = javaPath, // JAVA路径
                    Resolution = new ResolutionModel { Height = 480, Width = 854 }, // 游戏窗口分辨率
                    MinMemory = 1024, // 最小内存
                    MaxMemory = 4096, // 最大内存
                    GcType = GcType.G1Gc // GC类型
                },
                Authenticator = new OfflineAuthenticator //离线认证
                {
                    Username = "Offline User", //离线用户名
                    LauncherAccountParser = core.VersionLocator.LauncherAccountParser
                }
            };

            var result = await core.LaunchTaskAsync(launchSettings);
        }
        catch { return; }
    }
    [RelayCommand]
    private void GotoGame() => _navigationService.NavigateTo(typeof(GameViewModel).FullName!);
    [RelayCommand]
    private void GotoSettings() => _navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);



    public HomeViewModel(ILocalSettingsService localSettingsService, INavigationService navigationService)
    {
        _localSettingsService = localSettingsService;
        _navigationService = navigationService;

        Task.Run(InitializeAsync);
    }
    public async Task InitializeAsync()
    {
        BackgroundImageUrl = await _localSettingsService.ReadSettingAsync<string>("BackgroundImageUrl") ?? "/";
    }
}
