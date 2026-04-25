using Gnosis.Asset.Format;
using Gnosis.Core.Platform;
using Gnosis.Toolchain.AssetPipeline;
using Gnosis.Toolchain.Cooker.Cook;

namespace Genesis.Toolchain;

public sealed class GenesisBuildResult
{
    public required int TotalAssets { get; init; }

    public required int SucceededCount { get; init; }

    public required int FailedCount { get; init; }

    public required int UpToDateCount { get; init; }

    public required int SkippedCount { get; init; }

    public required TimeSpan TotalDuration { get; init; }

    public required IReadOnlyList<string> FailedAssets { get; init; }

    public required IReadOnlyList<string> OutputBundles { get; init; }

    public bool HasErrors => FailedCount > 0;

    public bool IsSuccess => FailedCount == 0;
}
