using ProjBobcat.Class.Helper;
using ProjBobcat.Class.Model;
using ProjBobcat.Class.Model.LauncherProfile;
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

        public static ObservableCollection<VersionInfo> GetAllGames(ObservableCollection<VersionInfo> oParam_0 = null!, bool bParam_1 = false)
        {
            if (RefetchAllProfiles(bParam_1))
            {
                return new(core.VersionLocator.GetAllGames());
            }
            else
                return oParam_0;
        }

        public static GameProfileModel? GetGameProfileModel(string name)
        {
            var profileDict = core.VersionLocator
                                                               .LauncherProfileParser!
                                                               .LauncherProfile
                                                               .Profiles;
            if (profileDict!.Count is > 0)
            {
                for (int i = 0; i < profileDict.Count; i++)
                {
                    KeyValuePair<string, GameProfileModel> pair = profileDict.ElementAt(i);
                    if (pair.Value.Name == name)
                    {
                        return pair.Value;
                    }
                }

                return default!;
            }
            else return default!;
        }

        public static bool RefetchAllProfiles(bool removeUnusedProfile = false)
        {
            var di = new DirectoryInfo(GamePathHelper.GetVersionPath(core.RootPath));

            var profiles = core.VersionLocator
                                                            .LauncherProfileParser!
                                                            .LauncherProfile
                                                            .Profiles!;

            foreach (DirectoryInfo folder in di.GetDirectories())
            {
                if (profiles.Count > 0)
                {
                    bool isRepeated = false;

                    for (int i = 0; i < profiles.Count; i++)
                    {
                        KeyValuePair<string, GameProfileModel> pair = profiles.ElementAt(i);

                        if (folder.Name == pair.Value.Name)
                        {
                            isRepeated = true;
                            goto END_LOOP;
                        }
                    }
                END_LOOP:
                    if (isRepeated) continue;
                }

                string[] subFileNames = Directory.GetFiles(folder.FullName);

                foreach (string fileName in subFileNames)
                {
                    if (Path.GetExtension(fileName) != ".json")
                    {
                        continue;
                    }
                    if (folder.Name == Path.GetFileNameWithoutExtension(fileName))
                    {
                        var name = folder.Name;
                        core.VersionLocator.LauncherProfileParser.LauncherProfile.Profiles!.Add(name.ToGuidHash().ToString("N"),
                        new GameProfileModel
                        {
                            GameDir = folder.FullName,
                            LastVersionId = name,
                            Name = name,
                            Created = DateTime.Now
                        });
                        core.VersionLocator.LauncherProfileParser.SaveProfile();
                    }
                }
            }

            if (removeUnusedProfile is true)
            {
                if (profiles.Count > 0)
                {
                    var removeList = new List<string>();

                    for (int i = 0; i < profiles.Count; i++)
                    {
                        var pair = profiles.ElementAt(i);

                        bool matchFound = false;

                        foreach (DirectoryInfo folder in di.GetDirectories())
                        {
                            if (folder.Name == pair.Value.Name)
                            {
                                matchFound = true;
                            }
                        }

                        if (!matchFound)
                        {
                            removeList.Add(pair.Key);
                        }
                    }

                    foreach (string item in removeList)
                    {
                        core.VersionLocator.LauncherProfileParser!.RemoveGameProfile(item);
                    }
                    core.VersionLocator.LauncherProfileParser!.SaveProfile();
                }
            }

            return true;
        }

        public static void RenameGameProfile(string name, string nameAfter)
        {
            var profiles = core.VersionLocator
                                                            .LauncherProfileParser!
                                                            .LauncherProfile
                                                            .Profiles!;
            if (profiles.Count is > 0)
            {
                for (int i = 0; i < profiles!.Count; i++)
                {
                    KeyValuePair<string, GameProfileModel> pair = profiles.ElementAt(i);
                    if (pair.Value.Name == name)
                    {
                        var versionPath = Path.Combine(GamePathHelper.GetVersionPath(core.RootPath));

                        string[] fileNames = Directory.GetFiles(Path.Combine(versionPath, name));

                        foreach (string fileName in fileNames)
                        {
                            if (Path.GetFileNameWithoutExtension(fileName) == name)
                            {
                                try
                                {
                                    string nameAfterWithExtension = Path.GetExtension(fileName) switch
                                    {
                                        ".json" => $"{nameAfter}.json",
                                        ".jar" => $"{nameAfter}.jar",
                                        _ => throw new ArgumentException("Invalid extension name.")
                                    };

                                    File.Move(fileName, Path.Combine(versionPath, name, nameAfterWithExtension));
                                }
                                catch (ArgumentException)
                                {
                                    continue;
                                }
                            }
                        }

                        Directory.Move(
                            Path.Combine(GamePathHelper.GetVersionPath(core.RootPath), name),
                            Path.Combine(GamePathHelper.GetVersionPath(core.RootPath), nameAfter
                        ));

                        pair.Value.Name = nameAfter;
                    }
                }
                core.VersionLocator.LauncherProfileParser.SaveProfile();
            }
        }

        public static void RemoveGameProfile(string name)
        {
            var profileDict = core.VersionLocator
                                                               .LauncherProfileParser!
                                                               .LauncherProfile
                                                               .Profiles;
            string identifyName = name;
            foreach (KeyValuePair<string, GameProfileModel> pair in profileDict!)
            {
                if (pair.Value.Name == name)
                {
                    if (pair.Value.Name == pair.Key)
                    {
                        identifyName = pair.Value.Name;
                    }
                    else
                    {
                        identifyName = pair.Key;
                    }
                }
            }
            core.VersionLocator.LauncherProfileParser!.RemoveGameProfile(identifyName);
            core.VersionLocator.LauncherProfileParser!.SaveProfile();
        }

        [Obsolete("Use GuidHash as the profile identifier.")]
        public static void ResetAllProfilesIdentifier()
        {
            var profileDict = core.VersionLocator.LauncherProfileParser!.LauncherProfile.Profiles;
            var currentCount = profileDict?.Count;
            if (currentCount is > 0)
            {
                for (int i = 0; i < currentCount; i++)
                {
                    KeyValuePair<string, GameProfileModel> pair = profileDict.ElementAt(i);
                    if (pair.Key != pair.Value.Name)
                    {
                        profileDict.Add(pair.Value.Name!, pair.Value);
                        profileDict.Remove(pair.Key);
                    }
                }
                core.VersionLocator.LauncherProfileParser.SaveProfile();
            }
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
}
