using ProjBobcat.Class.Model;
using ProjBobcat.DefaultComponent;
using ProjBobcat.DefaultComponent.Launch.GameCore;
using ProjBobcat.DefaultComponent.ResourceInfoResolver;
using ProjBobcat.Interface;

namespace CarpLauncher.Core;
public class ResourceCompleter
{
    private static readonly DefaultGameCore core = Core.GetGameCore();

    public static async Task<bool> DownloadResourcesAsync(VersionInfo versionInfo, Action<double, string> callback)
    {
        var versions = await Core.GetVersionManifestTaskAsync();
        var rc = new DefaultResourceCompleter
        {
            CheckFile = true,
            DownloadParts = 16,
            ResourceInfoResolvers = new List<IResourceInfoResolver>
            {
                new VersionInfoResolver
                {
                    BasePath = core.RootPath,
                    VersionInfo = versionInfo,
                    CheckLocalFiles = true
                },
                new AssetInfoResolver
                {
                    BasePath = core.RootPath,
                    VersionInfo = versionInfo,
                    CheckLocalFiles = true,
                    Versions = versions?.Versions
                },
                new LibraryInfoResolver
                {
                    BasePath = core.RootPath,
                    VersionInfo = versionInfo,
                    CheckLocalFiles = true,
                }
            },
            MaxDegreeOfParallelism = 8,
            TotalRetry = 3
        };

        rc.GameResourceInfoResolveStatus += (_, args)
            => callback.Invoke(args.Progress, args.Status ?? string.Empty);

        try
        {
            var result = await rc.CheckAndDownloadTaskAsync();

            if (result.TaskStatus == TaskResultStatus.Error && (result.Value?.IsLibDownloadFailed ?? false))
            {
                throw new Exception();
            }
            else { return true; }
        }
        catch { return false; }
    }
}
