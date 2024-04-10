using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjBobcat.Class.Model;
using ProjBobcat.DefaultComponent.Launch.GameCore;
using ProjBobcat.DefaultComponent.ResourceInfoResolver;
using ProjBobcat.DefaultComponent;
using ProjBobcat.Interface;

namespace CarpLauncher.Core;
public class ResourceCompleter
{
    private static DefaultGameCore core = Core.GetGameCore();

    public static async Task<bool> DownloadResourcesAsync(VersionInfo versionInfo)
    {
        var versions = await Core.GetVersionManifestTaskAsync();
        var rc = new DefaultResourceCompleter
        {
            CheckFile = true,
            DownloadParts = 8,
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
                        ForgeMavenUriRoot = "https://bmclapi2.bangbang93.com/maven",
                        VersionInfo = versionInfo,
                        CheckLocalFiles = true,
                    }
                },
            MaxDegreeOfParallelism = 8,
            TotalRetry = 3
        };

        try
        {
            var result = await rc.CheckAndDownloadTaskAsync();

            if (result.TaskStatus == TaskResultStatus.Error && (result.Value?.IsLibDownloadFailed ?? false))
            {
                // 在完成补全后，资源检查器会返回执行结果。
                // 您可以检查 result 中的属性值来确定补全是否完成
                throw new Exception();
                // IsLibDownloadFailed 会反映启动必须的库文件是否已经成功补全
                // 通常来说，如果库文件的补全失败，很有可能会导致游戏的启动失败
            }
            else { return true; }
        }
        catch { return false; }
    }
}
