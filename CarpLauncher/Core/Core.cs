using CarpLauncher.Contracts.Services;
using CarpLauncher.Helpers;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using ProjBobcat.Class.Helper;
using ProjBobcat.Class.Model.Mojang;
using ProjBobcat.DefaultComponent.Launch;
using ProjBobcat.DefaultComponent.Launch.GameCore;
using ProjBobcat.DefaultComponent.Logging;

namespace CarpLauncher.Core;
public class Core
{
    private static DefaultGameCore core = null!;

    public static async Task InitLaunchCore(string path)
    {
        var clientToken = new Guid("88888888-8888-8888-8888-888888888888");
        var rootPath = path;

        try
        {
            core = new DefaultGameCore
            {
                ClientToken = clientToken,
                RootPath = rootPath,
                VersionLocator = new DefaultVersionLocator(rootPath, clientToken)
                {
                    LauncherProfileParser = new DefaultLauncherProfileParser(rootPath, clientToken),
                    LauncherAccountParser = new DefaultLauncherAccountParser(rootPath, clientToken)
                },
                GameLogResolver = new DefaultGameLogResolver()
            };
        }
        catch (Exception)
        {
            try
            {
                File.Delete($@"{rootPath}\launcher_profiles.json");

                core = new DefaultGameCore
                {
                    ClientToken = clientToken,
                    RootPath = rootPath,
                    VersionLocator = new DefaultVersionLocator(rootPath, clientToken)
                    {
                        LauncherProfileParser = new DefaultLauncherProfileParser(rootPath, clientToken),
                        LauncherAccountParser = new DefaultLauncherAccountParser(rootPath, clientToken)
                    },
                    GameLogResolver = new DefaultGameLogResolver()
                };
            }
            catch
            {
                var defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.minecraft";
                await DialogHelper.ShowRegularContentDialogAsync("Error", "Invalid .minecraft path detected!");
                await App.GetService<ILocalSettingsService>().SaveSettingAsync("GameRootPath", defaultPath);
                core = new DefaultGameCore
                {
                    ClientToken = clientToken,
                    RootPath = defaultPath,
                    VersionLocator = new DefaultVersionLocator(defaultPath, clientToken)
                    {
                        LauncherProfileParser = new DefaultLauncherProfileParser(defaultPath, clientToken),
                        LauncherAccountParser = new DefaultLauncherAccountParser(defaultPath, clientToken)
                    },
                    GameLogResolver = new DefaultGameLogResolver()
                };              
            }
        }

        VersionManifest = await HttpManager.GetVersionManifest() ?? GameHelper.GetDefaultVersionManifest();

        GameHelper.Initialize(core);
        DownloadManager.Initialize(core);
        ForgeInstaller.Initialize(core);
        FabricInstaller.Initialize(core);
    }

    public static DefaultGameCore GetGameCore()
    {
        return core;
    }

    public static List<string> VersionManifest { get; set; } = null!;

    public static async Task<VersionManifest> GetVersionManifestTaskAsync()
    {
        const string vmUrl = "http://launchermeta.mojang.com/mc/game/version_manifest.json";
        var contentRes = await HttpHelper.Get(vmUrl);
        var content = await contentRes.Content.ReadAsStringAsync();
        var model = JsonConvert.DeserializeObject<VersionManifest>(content);

        return model!;
    }
}
