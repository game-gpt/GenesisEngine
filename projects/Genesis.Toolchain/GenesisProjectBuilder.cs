using Gnosis.Asset.Format;
using Gnosis.Core.Platform;

namespace Genesis.Toolchain;

public sealed class GenesisProjectBuilder
{
    #region 字段

    private readonly IFormatRegistry _formatRegistry;

    #endregion

    #region 构造函数

    public GenesisProjectBuilder(IFormatRegistry formatRegistry)
    {
        _formatRegistry = formatRegistry ?? throw new ArgumentNullException(nameof(formatRegistry));
    }

    #endregion

    #region 公开方法

    public async Task<GenesisBuildResult> BuildProjectAsync(
        string projectRoot,
        PlatformType targetPlatform = PlatformType.Windows,
        string? outputRoot = null,
        CancellationToken cancellationToken = default)
    {
        var contentRoot = Path.Combine(projectRoot, "content");
        var output = outputRoot ?? Path.Combine(projectRoot, "build", targetPlatform.ToString().ToLowerInvariant());
        var cacheDir = Path.Combine(projectRoot, ".cache", "build");

        var options = new GenesisBuildOptions
        {
            ProjectRoot = projectRoot,
            ContentRoot = contentRoot,
            OutputRoot = output,
            CacheDirectory = cacheDir,
            TargetPlatform = targetPlatform
        };

        using var pipeline = new GenesisBuildPipeline(_formatRegistry);
        pipeline.Initialize(options);

        return await pipeline.BuildAsync(options, cancellationToken);
    }

    public async Task<GenesisBuildResult> BuildProjectForAllPlatformsAsync(
        string projectRoot,
        string? outputRoot = null,
        CancellationToken cancellationToken = default)
    {
        var platforms = new[]
        {
            PlatformType.Windows,
            PlatformType.Linux,
            PlatformType.macOS,
            PlatformType.Android,
            PlatformType.WebAssembly
        };

        var results = new List<GenesisBuildResult>();
        var allFailedAssets = new List<string>();
        var allOutputBundles = new List<string>();
        var totalAssets = 0;
        var totalSucceeded = 0;
        var totalFailed = 0;
        var totalUpToDate = 0;
        var totalSkipped = 0;

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        foreach (var platform in platforms)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await BuildProjectAsync(projectRoot, platform, outputRoot, cancellationToken);
            results.Add(result);

            totalAssets += result.TotalAssets;
            totalSucceeded += result.SucceededCount;
            totalFailed += result.FailedCount;
            totalUpToDate += result.UpToDateCount;
            totalSkipped += result.SkippedCount;
            allFailedAssets.AddRange(result.FailedAssets);
            allOutputBundles.AddRange(result.OutputBundles);
        }

        stopwatch.Stop();

        return new GenesisBuildResult
        {
            TotalAssets = totalAssets,
            SucceededCount = totalSucceeded,
            FailedCount = totalFailed,
            UpToDateCount = totalUpToDate,
            SkippedCount = totalSkipped,
            TotalDuration = stopwatch.Elapsed,
            FailedAssets = allFailedAssets.Distinct().ToList(),
            OutputBundles = allOutputBundles
        };
    }

    #endregion
}
