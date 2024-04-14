using CarpLauncher.Helpers;
using ProjBobcat.Class.Helper;
using ProjBobcat.Class.Model;
using ProjBobcat.Class.Model.LauncherProfile;
using ProjBobcat.DefaultComponent.Launch.GameCore;

namespace CarpLauncher.Core;

public class DownloadManager
{
    private static DefaultGameCore core = null!;

    public static void Initialize(DefaultGameCore gameCore)
    {
        core = gameCore!;
    }

    public static void DownloadMinecraft(string id, Minecraft client = Minecraft.Vanilla)
    {
        switch (client)
        {
            case Minecraft.Vanilla:
                break;

            case Minecraft.Forge:
                break;

            case Minecraft.Fabric:
                break;
        }
    }

    public static async Task DownloadMinecraftVanillaAsync(string id)
    {
        if (id is null)
        {
            await DialogHelper.ShowRegularContentDialogAsync("Error", "Choose a version before click download");
            return;
        }

        if (core.VersionLocator.GetGame(id)==null)
        {
            var dialog = await DialogHelper.GetProgressDialog("Downloading...", closeBtnText: "Hide");

            dialog.ShowAsync();

            // part 1
            string apiUrl = $"https://bmclapi2.bangbang93.com/version/{id}";
            string path = core.RootPath + "\\versions\\" + id;
            core.VersionLocator.LauncherProfileParser?.AddNewGameProfile(new GameProfileModel
            {
                Name = id,
                GameDir = path,
            });

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path + "\\" + id + ".jar")!);
                var jarResponse = await HttpHelper.Get(apiUrl + "/client");
                byte[] jarContent = await jarResponse.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes(path + "\\" + id + ".jar", jarContent);

                Directory.CreateDirectory(Path.GetDirectoryName(path + "\\" + id + ".json")!);
                var jsonResponse = await HttpHelper.Get(apiUrl + "/json");
                string jsonContent = await jsonResponse.Content.ReadAsStringAsync();
                File.WriteAllText(path + "\\" + id + ".json", jsonContent);
            }
            catch
            {
                dialog.Hide();
                await DialogHelper.ShowRegularContentDialogAsync("Error", "Critical Error occurred while downloading Jar files");
                return;
            }

            // part 2
            VersionInfo? versionInfo = core.VersionLocator.GetGame(id);
            try
            {
                bool result = await ResourceCompleter.DownloadResourcesAsync(versionInfo);
                if (result == false)
                {
                    throw new InvalidDataException();
                }
            }
            catch
            {
                dialog.Hide();
                await DialogHelper.ShowRegularContentDialogAsync("Error", "Critical Error occurred while completing resources");
                Directory.Delete(path, true);
                return;
            }

            dialog.Hide();
            await DialogHelper.ShowRegularContentDialogAsync("Done", "All files sucessfully downloaded!");
        }
        else
        {
            await DialogHelper.ShowRegularContentDialogAsync("Error", "Version already existed !");
            return;
        }
    }

    public static async Task DownloadMinecraftFabricAsync(string id)
    {
        if (core.VersionLocator.GetGame(id) == null)
        {
            var dialog = await DialogHelper.GetProgressDialog("Downloading...", closeBtnText: "Hide");

            dialog.ShowAsync();

            // part 1
            string apiUrl = $"https://bmclapi2.bangbang93.com/version/{id}";
            string path = $@"{core.RootPath}\versions\{id}";
            core.VersionLocator.LauncherProfileParser?.AddNewGameProfile(new GameProfileModel
            {
                Name = id,
                GameDir = path,
            });

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path + "\\" + id + ".jar")!);
                var jarResponse = await HttpHelper.Get(apiUrl + "/client");
                byte[] jarContent = await jarResponse.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes(path + "\\" + id + ".jar", jarContent);

                Directory.CreateDirectory(Path.GetDirectoryName(path + "\\" + id + ".json")!);
                var jsonResponse = await HttpHelper.Get(apiUrl + "/json");
                string jsonContent = await jsonResponse.Content.ReadAsStringAsync();
                File.WriteAllText(path + "\\" + id + ".json", jsonContent);
            }
            catch
            {
                dialog.Hide();
                await DialogHelper.ShowRegularContentDialogAsync("Error", "Critical Error occurred while downloading Jar files");
                return;
            }

            try
            {
                await FabricInstaller.InstallFabricAsync(id);
            }
            catch
            {
                dialog.Hide();
                await DialogHelper.ShowRegularContentDialogAsync("Error", "Critical Error occurred while downloading Fabric");
                return;
            }

            VersionInfo? versionInfo = core.VersionLocator.GetGame(id);
            try
            {
                bool result = await ResourceCompleter.DownloadResourcesAsync(versionInfo);
                if (result == false)
                {
                    throw new InvalidDataException();
                }
            }
            catch
            {
                dialog.Hide();
                await DialogHelper.ShowRegularContentDialogAsync("Error", "Critical Error occurred while completing resources");
                Directory.Delete(path, true);
                return;
            }

            dialog.Hide();
            await DialogHelper.ShowRegularContentDialogAsync("Done", "All files sucessfully downloaded!");
        }
        else
        {
            await DialogHelper.ShowRegularContentDialogAsync("Error", "Version already existed !");
            return;
        }
    }
}

public enum Source
{
    Mojang = 0, BMCLAPI = 1
}
