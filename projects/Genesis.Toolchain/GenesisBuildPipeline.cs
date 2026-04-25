using Gnosis.Asset.Format;
using Gnosis.Core.Platform;
using Gnosis.Toolchain.AssetPipeline;
using Gnosis.Toolchain.Cooker.Cook;

namespace Genesis.Toolchain;

public sealed class GenesisBuildPipeline : IDisposable
{
    #region 字段

    private readonly IFormatRegistry _formatRegistry;
    private AssetBuildPipeline? _assetPipeline;
    private PlatformCooker? _cooker;
    private bool _disposed;

    #endregion

    #region 属性

    public AssetBuildPipeline? AssetPipeline => _assetPipeline;

    public PlatformCooker? Cooker => _cooker;

    #endregion

    #region 构造函数

    public GenesisBuildPipeline(IFormatRegistry formatRegistry)
    {
        _formatRegistry = formatRegistry ?? throw new ArgumentNullException(nameof(formatRegistry));
    }

    #endregion

    #region 公开方法

    public void Initialize(GenesisBuildOptions options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var pipelineOptions = new BuildPipelineOptions
        {
            ContentRoot = options.ContentRoot,
            OutputRoot = options.OutputRoot,
            CacheDirectory = options.CacheDirectory,
            EnableIncrementalBuild = options.EnableIncrementalBuild,
            FailOnFirstError = options.FailOnFirstError,
            MaxDegreeOfParallelism = options.MaxDegreeOfParallelism
        };

        _assetPipeline = new AssetBuildPipeline(_formatRegistry, pipelineOptions);
        _cooker = new PlatformCooker(_formatRegistry);
    }

    public async Task<GenesisBuildResult> BuildAsync(GenesisBuildOptions options, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_assetPipeline is null || _cooker is null)
        {
            throw new InvalidOperationException("构建管线未初始化，请先调用 Initialize()");
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        _assetPipeline.DiscoverAssets(options.ContentRoot);
        _assetPipeline.BuildDependencyGraph();

        var assetResult = await _assetPipeline.BuildAsync(cancellationToken);

        var cookOptions = new CookOptions
        {
            TargetPlatform = options.TargetPlatform,
            TextureCompression = _cooker.GetDefaultTextureCompression(options.TargetPlatform),
            AudioEncoding = _cooker.GetDefaultAudioEncoding(options.TargetPlatform),
            StripDebugInfo = options.StripDebugInfo,
            EnableCompression = options.EnableCompression,
            EnableEncryption = options.EnableEncryption
        };

        var outputBundles = new List<string>();
        var bundleName = $"genesis_{options.TargetPlatform.ToString().ToLowerInvariant()}";

        if (assetResult.SucceededCount > 0)
        {
            var succeededAssets = assetResult.Results
                .Where(r => r.Success && r.OutputPath is not null)
                .Select(r => r.OutputPath!)
                .ToList();

            if (succeededAssets.Count > 0)
            {
                var bundleResult = await _cooker.CookAndPackAsync(
                    succeededAssets,
                    bundleName,
                    options.OutputRoot,
                    cookOptions,
                    cancellationToken
                );

                if (bundleResult.PackFilePath is not null)
                {
                    outputBundles.Add(bundleResult.PackFilePath);
                }
            }
        }

        stopwatch.Stop();

        return new GenesisBuildResult
        {
            TotalAssets = assetResult.TotalAssets,
            SucceededCount = assetResult.SucceededCount,
            FailedCount = assetResult.FailedCount,
            UpToDateCount = assetResult.UpToDateCount,
            SkippedCount = assetResult.SkippedCount,
            TotalDuration = stopwatch.Elapsed,
            FailedAssets = assetResult.FailedAssets,
            OutputBundles = outputBundles
        };
    }

    public void InvalidateAsset(string assetPath)
    {
        _assetPipeline?.InvalidateAsset(assetPath);
    }

    public void InvalidateAll()
    {
        _assetPipeline?.InvalidateAll();
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _assetPipeline?.Dispose();
        _disposed = true;
    }

    #endregion
}
