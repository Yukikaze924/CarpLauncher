using ProjBobcat.Class.Model;
using ProjBobcat.DefaultComponent.Launch.GameCore;
using System.Collections.ObjectModel;

namespace CarpLauncher.Core
{
    public static class GameHelper
    {
        private static DefaultGameCore core = null!;

        public static void Initialize(DefaultGameCore gameCore)
        {
            core = gameCore;
        }

        public static ObservableCollection<VersionInfo> GetAllGames(ObservableCollection<VersionInfo> before)
        {
            ObservableCollection<VersionInfo> games;
            try
            {
                games = new ObservableCollection<VersionInfo>(core.VersionLocator.GetAllGames());
            }
            catch
            {
                return before;
            }

            return games;
        }

        public static List<string> GetDefaultVersionManifest()
        {
            return new List<string>
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
        }
    }

    public enum Minecraft
    {
        Vanilla = 0,
        Forge = 1,
        Fabric = 2
    }
}
