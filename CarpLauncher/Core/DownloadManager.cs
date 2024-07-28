using CarpLauncher.Controls;
using CarpLauncher.Helpers;
using Microsoft.UI.Xaml.Controls;
using ProjBobcat.Class.Model;
using ProjBobcat.DefaultComponent.Launch.GameCore;

namespace CarpLauncher.Core;

public class DownloadManager
{
    private static DefaultGameCore core = null!;

    public static void Initialize(DefaultGameCore gameCore)
    {
        core = gameCore!;
    }

    public static async Task DownloadMinecraftVanillaAsync(string version, string name)
    {
        if (version is null)
        {
            await DialogHelper.ShowRegularContentDialogAsync("Error", "Choose a version before click download");
            return;
        }

        name ??= version;

        if (core.VersionLocator.GetGame(name) is not null)
        {
            return;
        }

        string apiUrl = $"https://bmclapi2.bangbang93.com/version/{version}";
        string path = Path.Combine(core.RootPath, "versions", name);

        var dialog = DialogHelper.GetProgressDialog("Preparing...", closeBtnText: "Hide");
        var content = (ProgressDialog)dialog.Content;

        switch (0)
        {
            case 0:
                {
                    _ = dialog.ShowAsync();

                    App.watcher.EnableRaisingEvents = false;

                    Directory.CreateDirectory(path);

                    goto case 1;
                }

            case 1:
                {
                    try
                    {
                        await HttpManager.DownloadAsync(apiUrl + "/client", path, name + ".jar", (progress) =>
                        {
                            dialog.Title = $"Downloading {name}.jar ({progress}%)";
                            content.ReportProgress(progress);
                        });
                    }
                    catch
                    {
                        goto case -1;
                    }

                    goto case 2;
                }

            case 2:
                {
                    try
                    {
                        await HttpManager.DownloadAsync(apiUrl + "/json", path, name + ".json", (progress) =>
                        {
                            dialog.Title = $"Downloading {name}.json ({progress}%)";
                            content.ReportProgress(progress);
                        });
                    }
                    catch
                    {
                        goto case -2;
                    }

                    goto case 3;
                }

            case 3:
                {
                    GameHelper.RefetchAllProfiles();

                    try
                    {
                        VersionInfo? versionInfo = core.VersionLocator.GetGame(name);

                        dialog.Title = $"Completing resources...";
                        content.IsProgressIndeterminate(true);

                        bool result = await ResourceCompleter.DownloadResourcesAsync(versionInfo!, (progress, status) =>
                        {

                        });
                        if (!result)
                        {
                            goto case -3;
                        }
                    }
                    catch
                    {
                        goto case -3;
                    }

                    break;
                }

            case -1:
                {
                    var result = await DialogHelper.ShowRegularContentDialogAsync("Failed", "Error occurred while downloading client.jar.");
                    if (result is ContentDialogResult.Primary)
                    {
                        _ = dialog.ShowAsync();
                        goto case 1;
                    }
                    else
                    {
                        goto case -0x1a;
                    }
                }

            case -2:
                {
                    var result = await DialogHelper.ShowRegularContentDialogAsync("Failed", "Error occurred while downloading client.json.");
                    if (result is ContentDialogResult.Primary)
                    {
                        _ = dialog.ShowAsync();
                        goto case 2;
                    }
                    else
                    {
                        goto case -0x1a;
                    }
                }

            case -3:
                {
                    var result = await DialogHelper.ShowRegularContentDialogAsync("Failed", "Error occurred while completing resources.");
                    if (result is ContentDialogResult.Primary)
                    {
                        _ = dialog.ShowAsync();
                        goto case 3;
                    }
                    else
                    {
                        goto case -0x1a;
                    }
                }

            case -0x1a:
                {
                    try
                    {
                        Directory.Delete(path, true);
                    }
                    catch
                    {
                        if (Directory.Exists(path))
                        {
                            goto case -0x1a;
                        }
                    }

                    App.watcher.EnableRaisingEvents = false;

                    GameHelper.RemoveGameProfile(name);

                    return;
                }
        }

        dialog.Hide();
        await DialogHelper.ShowRegularContentDialogAsync("Done", "All files sucessfully downloaded!");
    }

    public static async Task DownloadMinecraftFabricAsync(string version, string name)
    {


        // part 1
        string apiUrl = $"https://bmclapi2.bangbang93.com/version/{version}";
        string path = $@"{core.RootPath}\versions\{version}";

        var dialog = DialogHelper.GetIndeterminateProgressDialog("Downloading...", closeBtnText: "Hide");

        switch (0)
        {
            case 0:
                {
                    _ = dialog.ShowAsync();

                    App.watcher.EnableRaisingEvents = false;

                    goto case 1;
                }

            case 1:
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                        //
                        await HttpManager.DownloadAsync(apiUrl + "/client", path, version + ".jar", (p) =>
                        {
                            dialog.Title = $"Downloading client.jar ({p}%)";
                        });
                        //
                        await HttpManager.DownloadAsync(apiUrl + "/json", path, version + ".json", (p) =>
                        {
                            dialog.Title = $"Downloading client.json ({p}%)";
                        });
                    }
                    catch
                    {
                        goto case -1;
                    }

                    goto case 2;
                }

            case 2:
                {
                    dialog.Title = $"Installing Fabric";
                    try
                    {
                        await FabricInstaller.InstallFabricAsync(version);
                    }
                    catch
                    {
                        goto case -2;
                    }

                    goto case 3;
                }

            case 3:
                {
                    GameHelper.RefetchAllProfiles();

                    try
                    {
                        var McVersionInfo = core.VersionLocator.GetGame(version);
                        var McResult = await ResourceCompleter.DownloadResourcesAsync(McVersionInfo, (p, s) =>
                        {
                            dialog.Title = $"{s} ({p}%)";
                        });

                        var ForgeVersionInfo = core.VersionLocator.GetGame("1.19.2-fabric 0.16.0");
                        var ForgeResult = await ResourceCompleter.DownloadResourcesAsync(ForgeVersionInfo, (p, s) =>
                        {
                            dialog.Title = $"{s} ({p}%)";
                        });

                        if (McResult == false || ForgeResult == false)
                        {
                            goto case -3;
                        }
                    }
                    catch
                    {
                        goto case -3;
                    }

                    break;
                }

            case -1:
                {
                    dialog.Hide();
                    var result = await DialogHelper.ShowRegularContentDialogAsync("Failed", "Error occurred while downloading Client.", "Retry", "Cancel");
                    if (result is ContentDialogResult.Primary)
                    {
                        _ = dialog.ShowAsync();
                        goto case 1;
                    }
                    else
                    {
                        return;
                    }
                }

            case -2:
                {
                    dialog.Hide();
                    var result = await DialogHelper.ShowRegularContentDialogAsync("Failed", "Error occurred while installing Fabric.", "Retry", "Cancel");
                    if (result is ContentDialogResult.Primary)
                    {
                        _ = dialog.ShowAsync();
                        goto case 2;
                    }
                    else
                    {
                        return;
                    }
                }

            case -3:
                {
                    dialog.Hide();
                    var result = await DialogHelper.ShowRegularContentDialogAsync("Failed", "Error occurred while completing resources.", "Retry", "Cancel");
                    if (result is ContentDialogResult.Primary)
                    {
                        _ = dialog.ShowAsync();
                        goto case 3;
                    }
                    else
                    {
                        return;
                    }
                }
        }

        dialog.Title = "Installing...";
        try
        {
            await FabricInstaller.InstallFabricAsync(version);
        }
        catch
        {
            dialog.Hide();
            await DialogHelper.ShowRegularContentDialogAsync("Error", "Critical Error occurred while downloading Fabric");
            return;
        }

        App.watcher.EnableRaisingEvents = false;

        dialog.Hide();
        await DialogHelper.ShowRegularContentDialogAsync("Done", "All files sucessfully downloaded!");
    }

    public static async Task DownloadMinecraftForgeAsync(string version, string forgeVersion, string? name = null)
    {
        if (version is null)
        {
            return;
        }

        if (core.VersionLocator.GetGame(version) is not null)
        {
            return;
        }

        string forgeInstallerVersion = $@"{version}-{forgeVersion}"; // 如果name为空用这个来做Forge的版本名
        string forgeInstallerName = $@"forge-{forgeInstallerVersion}-installer.jar";

        name ??= forgeInstallerVersion;

        string apiUrl = $@"https://bmclapi2.bangbang93.com/version/{version}";
        string forgeApiUrl = $@"https://bmclapi2.bangbang93.com/maven/net/minecraftforge/forge/{forgeInstallerVersion}/{forgeInstallerName}";

        string forgeMcPath = $@"{core.RootPath}\versions\{version}";
        string forgePath = $@"{core.RootPath}\versions\{forgeInstallerVersion}";

        var dialog = DialogHelper.GetIndeterminateProgressDialog("Preparing...", closeBtnText: "Hide");

        switch (0)
        {
            case 0:
                {
                    _ = dialog.ShowAsync();

                    //core.VersionLocator.LauncherProfileParser?.AddNewGameProfile(new GameProfileModel
                    //{
                    //    Name = version,
                    //    GameDir = forgeMcPath,
                    //});
                    //core.VersionLocator.LauncherProfileParser?.AddNewGameProfile(new GameProfileModel
                    //{
                    //    Name = forgeInstallerVersion,
                    //    GameDir = forgePath,
                    //});

                    goto case 1;
                }

            case 1:
                {
                    try
                    {
                        Directory.CreateDirectory(forgeMcPath);
                        //
                        await HttpManager.DownloadAsync(apiUrl + "/client", forgeMcPath, version + ".jar", (p) =>
                        {
                            dialog.Title = $"Downloading client.jar ({p}%)";
                        });
                        //
                        await HttpManager.DownloadAsync(apiUrl + "/json", forgeMcPath, version + ".json", (p) =>
                        {
                            dialog.Title = $"Downloading client.json ({p}%)";
                        });
                    }
                    catch
                    {
                        goto case -1;
                    }

                    goto case 2;
                }

            case 2:
                {
                    try
                    {
                        Directory.CreateDirectory(forgePath);
                        await HttpManager.DownloadAsync(forgeApiUrl, forgePath, forgeInstallerName, (p) =>
                        {
                            dialog.Title = $"Downloading Forge-Installer ({p}%)";
                        });
                    }
                    catch
                    {
                        goto case -2;
                    }

                    goto case 3;
                }

            case 3:
                {
                    try
                    {
                        await ForgeInstaller.InstallForgeAsync(version, forgeVersion, forgeInstallerName, name, (progress, status) => dialog.Title = $"{status} ({progress}%)");
                    }
                    catch
                    {
                        goto case -3;
                    }

                    goto case 4;
                }

            case 4:
                {
                    GameHelper.RefetchAllProfiles();

                    try
                    {
                        var McVersionInfo = core.VersionLocator.GetGame(version);
                        var McResult = await ResourceCompleter.DownloadResourcesAsync(McVersionInfo, (p, s) =>
                        {
                            dialog.Title = $"{s} ({p}%)";
                        });

                        var ForgeVersionInfo = core.VersionLocator.GetGame(forgeInstallerVersion);
                        var ForgeResult = await ResourceCompleter.DownloadResourcesAsync(ForgeVersionInfo, (p, s) =>
                        {
                            dialog.Title = $"{s} ({p}%)";
                        });

                        if (McResult == false || ForgeResult == false)
                        {
                            goto case -4;
                        }
                    }
                    catch
                    {
                        goto case -4;
                    }

                    break;
                }

            case -1:
                {
                    dialog.Hide();
                    var result = await DialogHelper.ShowRegularContentDialogAsync("Failed", "Error occurred while downloading Client.", "Retry", "Cancel");
                    if (result is ContentDialogResult.Primary)
                    {
                        _ = dialog.ShowAsync();
                        goto case 1;
                    }
                    else
                    {
                        goto case -0x1a;
                    }
                }

            case -2:
                {
                    dialog.Hide();
                    var result = await DialogHelper.ShowRegularContentDialogAsync("Failed", "Error occurred while downloading Forge.", "Retry");
                    if (result == ContentDialogResult.Primary)
                    {
                        _ = dialog.ShowAsync();
                        goto case 2;
                    }
                    else
                    {
                        goto case -0x1a;
                    }
                }

            case -3:
                {
                    dialog.Hide();
                    var result = await DialogHelper.ShowRegularContentDialogAsync("Failed", "Error occurred while installing Forge.", "Retry");
                    if (result == ContentDialogResult.Primary)
                    {
                        _ = dialog.ShowAsync();
                        goto case 3;
                    }
                    else
                    {
                        goto case -0x1b;
                    }
                }

            case -4:
                {
                    dialog.Hide();
                    var result = await DialogHelper.ShowRegularContentDialogAsync("Failed", "Error occurred while completing Resources(Assets\\Libs).", "Retry");
                    if (result == ContentDialogResult.Primary)
                    {
                        _ = dialog.ShowAsync();
                        goto case 4;
                    }
                    else
                    {
                        goto case -0x1b;
                    }
                }

            case -0x1a:
                {
                    try
                    {
                        Directory.Delete(forgeMcPath, true);
                    }
                    catch
                    {
                        if (Directory.Exists(forgeMcPath))
                        {
                            goto case -0x1a;
                        }
                    }

                    goto case -0x1c;
                }

            case -0x1b:
                {
                    try
                    {
                        Directory.Delete(forgeMcPath, true);
                        Directory.Delete(forgePath, true);
                    }
                    catch
                    {
                        if (Directory.Exists(forgeMcPath) || Directory.Exists(forgePath))
                        {
                            goto case -0x1b;
                        }
                    }

                    goto case -0x1c;
                }

            case -0x1c:
                {
                    //GameHelper.ResetAllProfilesIdentifier();

                    //core.VersionLocator.LauncherProfileParser!.RemoveGameProfile(version);
                    //core.VersionLocator.LauncherProfileParser!.RemoveGameProfile(forgeInstallerVersion);

                    //core.VersionLocator.LauncherProfileParser.SaveProfile();

                    GameHelper.RemoveGameProfile(version);
                    GameHelper.RemoveGameProfile(forgeInstallerVersion);

                    return;
                }
        }

        dialog.Title = "Done";
        dialog.Content = new Controls.ContentDialog("All files have been successfully downloaded!");
        dialog.CloseButtonText = "Close";
        return;
    }
}
