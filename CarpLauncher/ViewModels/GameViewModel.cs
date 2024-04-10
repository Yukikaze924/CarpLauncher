﻿using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using ProjBobcat.Class.Helper;
using ProjBobcat.Class.Model;
using ProjBobcat.Class.Model.LauncherProfile;
using ProjBobcat.DefaultComponent.Launch.GameCore;

namespace CarpLauncher.ViewModels;

public partial class GameViewModel : ObservableObject
{
    [ObservableProperty]
    private List<string> _minecraftVersionList = new()
    {
        "1.7.10",
        "1.7.11",
        "1.7.12",
        "1.8.0",
        "1.8.1",
        "1.8.2",
        "1.8.3",
        "1.8.4",
        "1.8.5",
        "1.8.6",
        "1.8.7",
        "1.8.8",
        "1.8.9",
        "1.9.0",
        "1.9.1",
        "1.9.2",
        "1.9.3",
        "1.9.4",
        "1.10.0",
        "1.10.1",
        "1.10.2",
        "1.11.0",
        "1.11.1",
        "1.11.2",
        "1.12.0",
        "1.12.1",
        "1.12.2",
        "1.13.0",
        "1.13.1",
        "1.13.2",
        "1.14.0",
        "1.14.1",
        "1.14.2",
        "1.14.3",
        "1.14.4",
        "1.15.0",
        "1.15.1",
        "1.15.2",
        "1.16.0",
        "1.16.1",
        "1.16.2",
        "1.16.3",
        "1.16.4",
        "1.16.5",
        "1.17.0",
        "1.17.1",
        "1.18.0",
        "1.18.1",
        "1.18.2"
    };

    [ObservableProperty]
    private string _selectedMinecraftVersion;


    [ObservableProperty]
    private string _selectedForgeMinecraftVersion;
    [ObservableProperty]
    private ObservableCollection<string> _forgeVersionList = [];
    [ObservableProperty]
    private string _selectedForgeVersion;
    [ObservableProperty]
    private bool _isFailed = false;
    partial void OnSelectedForgeMinecraftVersionChanged(string value)
    {
        if (value is null) return;

        IsFailed = false;
        ForgeVersionList.Clear();

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
    private ObservableCollection<VersionInfo>? _profiles;
    private void _refreshProfiles()
    {
        //lock (_rwLock)
        //{
        //    Profiles = new ObservableCollection<VersionInfo>(Core.Core.GetGameCore().VersionLocator.GetAllGames().ToList());
        //}
        Profiles = new ObservableCollection<VersionInfo>(Core.Core.GetGameCore().VersionLocator.GetAllGames().ToList());
    }
    [RelayCommand]
    private void RefreshProfiles()
    {
        //Profiles = new ObservableCollection<VersionInfo>(core.VersionLocator.GetAllGames().ToList());
        _refreshProfiles();
    }


    private DefaultGameCore core = Core.Core.GetGameCore();
    private FileSystemWatcher watcher = new();


    public GameViewModel()
    {
        _refreshProfiles();

        watcher.Path = core.RootPath;
        watcher.IncludeSubdirectories = true;
        watcher.NotifyFilter = NotifyFilters.DirectoryName;
        watcher.Deleted += (sender, args) =>
        {
            var dispatcherQueue = (App.Current as App).DispatcherQueue;
            dispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    _refreshProfiles();
                }
                catch { return; }
            });
        };
        watcher.Created += (sender, args) =>
        {
            var dispatcherQueue = (App.Current as App).DispatcherQueue;
            dispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    _refreshProfiles();
                }
                catch { return; }
            });
        };
        watcher.EnableRaisingEvents = true;
    }



    [RelayCommand]
    private async Task DownloadVanillaMinecraftAsync()
    {
        if (core.VersionLocator.GetGame(SelectedMinecraftVersion) == null)
        {
            // part 1
            string apiUrl = "https://bmclapi2.bangbang93.com/version/" + SelectedMinecraftVersion;
            string path = core.RootPath + "\\versions\\" + SelectedMinecraftVersion;
            core.VersionLocator.LauncherProfileParser?.AddNewGameProfile(new GameProfileModel
            {
                Name = SelectedMinecraftVersion,
                GameDir = path,
            });
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path + "\\" + SelectedMinecraftVersion + ".jar"));
                var jarResponse = await HttpHelper.Get(apiUrl + "/client");
                byte[] jarContent = await jarResponse.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes(path + "\\" + SelectedMinecraftVersion + ".jar", jarContent);

                Directory.CreateDirectory(Path.GetDirectoryName(path + "\\" + SelectedMinecraftVersion + ".json"));
                var jsonResponse = await HttpHelper.Get(apiUrl + "/json");
                string jsonContent = await jsonResponse.Content.ReadAsStringAsync();
                File.WriteAllText(path + "\\" + SelectedMinecraftVersion + ".json", jsonContent);
            }
            catch
            {
                return;
            }

            // part 2
            VersionInfo? McVersionInfo = core.VersionLocator.GetGame(SelectedMinecraftVersion);
            try
            {
                bool result = await Core.ResourceCompleter.DownloadResourcesAsync(McVersionInfo);
                if (result == false)
                {
                    throw new Exception();
                }
            }
            catch
            {
                Directory.Delete(path, true);
                return;
            }
        }
        else
        {
            return;
        }
    }
}