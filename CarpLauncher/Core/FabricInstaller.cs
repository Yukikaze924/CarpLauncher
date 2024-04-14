using Newtonsoft.Json;
using ProjBobcat.Class.Helper;
using ProjBobcat.Class.Model.Fabric;
using ProjBobcat.DefaultComponent.Launch.GameCore;

namespace CarpLauncher.Core
{
    public class FabricInstaller
    {
        private static HttpClient httpClient = new ();
        private static DefaultGameCore core = null!;

        public static void Initialize(DefaultGameCore gameCore) => core = gameCore;

        public static async Task InstallFabricAsync(string version)
        {
            var selectedArtifact = await GetFabricArtifactAsync(version);

            var fabricInstaller = new ProjBobcat.DefaultComponent.Installer.FabricInstaller
            {
                LoaderArtifact = selectedArtifact!,
                VersionLocator = core.VersionLocator,
                RootPath = core.RootPath,
                CustomId = version,
                InheritsFrom = version
            };

            if (fabricInstaller == null ) { throw new NullReferenceException(); }

            await fabricInstaller.InstallTaskAsync();
        }

        private static async Task<FabricLoaderArtifactModel?> GetFabricArtifactAsync(string version)
        {
            var responese = await HttpHelper.Get($"https://meta.fabricmc.net/v2/versions/loader/{version}");

            var json = await responese.Content.ReadAsStringAsync();

            try
            {   // 在将 json 内容反序列化为FabricLoaderArtifactModel集合时抛出了异常
                var artifacts = JsonConvert.DeserializeObject<List<FabricLoaderArtifactModel>>(json);
                // 获取单个 Loader Artifact
                var selectedArtifact = artifacts[0];

                return selectedArtifact ?? default;
            }
            catch (Exception ex)
            {
                // 异常内容
                // Error converting value "net.minecraft.launchwrapper.Launch" to type 'System.Text.Json.JsonElement'.
                // Path '[196].launcherMeta.mainClass', line 13478, position 55.
            }

            return default;
        }
    }
}
