using Gnosis.Core.Platform;
using Gnosis.Toolchain.AssetPipeline;
using Gnosis.Toolchain.Cooker.Cook;

namespace Genesis.Toolchain;

public sealed class GenesisBuildOptions
{
    public string ProjectRoot { get; init; } = string.Empty;

    public string ContentRoot { get; init; } = string.Empty;

    public string OutputRoot { get; init; } = string.Empty;

    public string CacheDirectory { get; init; } = string.Empty;

    public PlatformType TargetPlatform { get; init; } = PlatformType.Windows;

    public bool EnableIncrementalBuild { get; init; } = true;

    public bool EnableCompression { get; init; } = true;

    public bool EnableEncryption { get; init; } = false;

    public bool StripDebugInfo { get; init; } = true;

    public int MaxDegreeOfParallelism { get; init; } = 4;

    public bool FailOnFirstError { get; init; } = false;
}
