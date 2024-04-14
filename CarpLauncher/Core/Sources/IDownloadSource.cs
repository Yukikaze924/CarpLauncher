﻿namespace CarpLauncher.Core.Sources
{
    public interface IDownloadSource
    {
        string? RootUri { get; }
        string VersionManifestJson { get; }
        string Assets { get; }
        string Libraries { get; }
        string Forge { get; }
        string ForgeMaven { get; }
        string ForgeMavenOld { get; }
        string FabricMaven { get; }
    }
}
