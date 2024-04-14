using CarpLauncher.Contracts.Services;
using CarpLauncher.Core;
using CarpLauncher.Core.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using ProjBobcat.Class.Model;
using ProjBobcat.Class.Model.LauncherProfile;
using ProjBobcat.DefaultComponent.Launch.GameCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace CarpLauncher.ViewModels;

public partial class GameViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly DefaultGameCore core = Core.Core.GetGameCore();

    [ObservableProperty]
    private List<string> _minecraftVersionList = Core.Core.VersionManifest;
    [ObservableProperty]
    private string? _selectedMinecraftVersion;
    [ObservableProperty]
    private string? _selectedForgeMinecraftVersion;
    [ObservableProperty]
    private ObservableCollection<string>? _forgeVersionList = [];
    [ObservableProperty]
    private string? _selectedForgeVersion;
    [ObservableProperty]
    private bool _isFailed = false;
    partial void OnSelectedForgeMinecraftVersionChanged(string value)
    {
        if (value is null) return;

        IsFailed = false;
        ForgeVersionList!.Clear();

        try
        {
            // Create a web request to the specified URL.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://bmclapi2.bangbang93.com/forge/minecraft/{SelectedForgeMinecraftVersion}");
            // Set the request method to GET.
            request.Method = "GET";
            // Get the web response.
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response != null)
            {
                // Read the response stream.
                string content = new StreamReader(response.GetResponseStream()).ReadToEnd();

                var json = JArray.Parse(content);

                if (json.Count == 0) throw new Exception();

                for (int i = json.Count - 1; i >= 0; i--)
                {
                    ForgeVersionList.Add(json[i]["version"].ToString());
                }
            }
            else
            {
                throw new Exception();
            }
        }
        catch
        {
            IsFailed = true;
            return;
        }
    }
    [ObservableProperty]
    private string? _selectedFabricVersion;
    [ObservableProperty]
    private ObservableCollection<VersionInfo>? _profiles;
    [ObservableProperty]
    private string? _profilesCountText;
    partial void OnProfilesChanged(ObservableCollection<VersionInfo>? value)
    {
        if (value is null) return;
        ProfilesCountText = $"Games ({value.Count})";
    }
    [ObservableProperty]
    private bool _isMinecraftReadyToDownload;
    partial void OnSelectedMinecraftVersionChanged(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) IsMinecraftReadyToDownload = false;
        else IsMinecraftReadyToDownload = true;
    }


    public void _refreshProfiles() => Profiles = GameHelper.GetAllGames(Profiles);
    [RelayCommand]
    private void RefreshProfiles() => _refreshProfiles();
    [RelayCommand]
    private void OpenGameFolder() => Process.Start(new ProcessStartInfo { FileName = core.RootPath, UseShellExecute = true, Verb = "open" });
    [RelayCommand]
    private void GotoSettings() => _navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
    [RelayCommand]
    private async Task ImportGameFromFolderAsync()
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
            try
            {
                IOHelper.CopyDirectory(folder.Path, $@"{core.RootPath}\versions\{folder.Name}");
            }
            catch
            {
                return;
            }
            core.VersionLocator.LauncherProfileParser?.AddNewGameProfile(new GameProfileModel
            {
                Name = folder.Name,
                GameDir = $@"{core.RootPath}\versions\{folder.Name}",
            });
        }
        else
        {
            return;
        }
    }


    public GameViewModel(INavigationService navigationService)
    {
        _refreshProfiles();

        _navigationService = navigationService;
    }



    [RelayCommand]
    private async Task DownloadVanillaMinecraftAsync()
    {
        await DownloadManager.DownloadMinecraftVanillaAsync(SelectedMinecraftVersion);
    }
    [RelayCommand]
    private async Task DownloadFabricMinecraftAsync()
    {
        await DownloadManager.DownloadMinecraftFabricAsync(SelectedFabricVersion);
    }
}
