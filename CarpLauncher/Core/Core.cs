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
    public static List<string> VersionManifest { get; set; } = null!;

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
            var result = await DialogHelper.ShowRegularContentDialogAsync
            ("Error", "Invalid launcher_profiles.json detected!\nDo you want to remove it?", "Yes");
            if (result == ContentDialogResult.Primary)
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
        }

        VersionManifest = await HttpManager.GetVersionManifest() ?? GameHelper.GetDefaultVersionManifest();

        GameHelper.Initialize(core);
        DownloadManager.Initialize(core);
        FabricInstaller.Initialize(core);
    }

    public static DefaultGameCore GetGameCore()
    {
        return core;
    }

    public static async Task<VersionManifest> GetVersionManifestTaskAsync()
    {
        const string vmUrl = "http://launchermeta.mojang.com/mc/game/version_manifest.json";
        var contentRes = await HttpHelper.Get(vmUrl);
        var content = await contentRes.Content.ReadAsStringAsync();
        var model = JsonConvert.DeserializeObject<VersionManifest>(content);

        return model;
    }
}
