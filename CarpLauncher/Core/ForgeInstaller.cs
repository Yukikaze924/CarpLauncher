using CarpLauncher.Contracts.Services;
using ProjBobcat.Class.Model;
using ProjBobcat.DefaultComponent.Installer.ForgeInstaller;
using ProjBobcat.DefaultComponent.Launch.GameCore;
using ProjBobcat.Interface;

namespace CarpLauncher.Core
{
    public class ForgeInstaller
    {
        private static DefaultGameCore core = null!;

        public static void Initialize(DefaultGameCore gameCore)
        {
            core = gameCore!;
        }

        public static async Task InstallForgeAsync(string MinecraftVersion, string ForgeVersion, string InstallerVersion, string name, Action<double, string> callback)
        {
            string? javaPath;
            try
            {
                javaPath = await App.GetService<ILocalSettingsService>().ReadSettingAsync<string>("JavaExecutablePath");
                ArgumentException.ThrowIfNullOrWhiteSpace(javaPath);
            }
            catch
            {
                return;
            }


            var forgeVersion = ForgeInstallerFactory.GetForgeArtifactVersion(MinecraftVersion, ForgeVersion);

            var forgeJarPath = core.RootPath + "\\versions\\" + forgeVersion + "\\" + InstallerVersion;

            var isLegacy = ForgeInstallerFactory.IsLegacyForgeInstaller(forgeJarPath, forgeVersion);

            IForgeInstaller forgeInstaller = isLegacy
            ? new LegacyForgeInstaller
            {
                ForgeExecutablePath = forgeJarPath,
                RootPath = core.RootPath,
                CustomId = forgeVersion,
                ForgeVersion = forgeVersion,
                InheritsFrom = MinecraftVersion
            }
            : new HighVersionForgeInstaller
            {
                ForgeExecutablePath = forgeJarPath,
                JavaExecutablePath = javaPath,
                RootPath = core.RootPath,
                VersionLocator = core.VersionLocator,
                DownloadUrlRoot = "https://bmclapi2.bangbang93.com/",
                CustomId = forgeVersion,
                MineCraftVersion = MinecraftVersion,
                MineCraftVersionId = MinecraftVersion,
                InheritsFrom = MinecraftVersion
            };

            ((InstallerBase)forgeInstaller).StageChangedEventDelegate += (_, args) =>
            {
                try
                {
                    ArgumentException.ThrowIfNullOrEmpty(args.CurrentStage);
                    callback.Invoke(args.Progress*100, args.CurrentStage);
                }
                catch
                {
                    return;
                }
            };

            await forgeInstaller.InstallForgeTaskAsync();
        }
    }
}
