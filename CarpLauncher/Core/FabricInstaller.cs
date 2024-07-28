using ProjBobcat.Class.Helper;
using ProjBobcat.Class.Model.Fabric;
using ProjBobcat.DefaultComponent.Launch.GameCore;
using System.Text.Json;

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

            if (selectedArtifact == null) { throw new NullReferenceException(); }

            var fabricInstaller = new ProjBobcat.DefaultComponent.Installer.FabricInstaller
            {
                LoaderArtifact = selectedArtifact!,
                VersionLocator = core.VersionLocator,
                RootPath = core.RootPath,
                CustomId = "1.19.2-fabric 0.16.0",
                InheritsFrom = version
            };

            await fabricInstaller.InstallTaskAsync();
        }

        private static async Task<FabricLoaderArtifactModel?> GetFabricArtifactAsync(string version)
        {
            var responese = await HttpHelper.Get($"https://meta.fabricmc.net/v2/versions/loader/{version}");

            var json = await responese.Content.ReadAsStringAsync();

            try
            {
                var artifacts = JsonSerializer.Deserialize<List<FabricLoaderArtifactModel>>(json);

                // 获取单个 Loader Artifact
                var selectedArtifact = artifacts[0];

                return selectedArtifact ?? default;
            }
            catch
            {
                return default;
            }
        }
    }
}
