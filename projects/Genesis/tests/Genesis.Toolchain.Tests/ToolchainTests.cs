using Gnosis.Asset.Format;
using Gnosis.Core.Platform;
using Genesis.Toolchain;
using Xunit;

namespace Genesis.Toolchain.Tests;

public class GenesisBuildOptionsTests
{
    #region 默认值测试

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var options = new GenesisBuildOptions();

        Assert.Equal(string.Empty, options.ProjectRoot);
        Assert.Equal(string.Empty, options.ContentRoot);
        Assert.Equal(string.Empty, options.OutputRoot);
        Assert.Equal(string.Empty, options.CacheDirectory);
        Assert.Equal(PlatformType.Windows, options.TargetPlatform);
        Assert.True(options.EnableIncrementalBuild);
        Assert.True(options.EnableCompression);
        Assert.False(options.EnableEncryption);
        Assert.True(options.StripDebugInfo);
        Assert.Equal(4, options.MaxDegreeOfParallelism);
        Assert.False(options.FailOnFirstError);
    }

    #endregion

    #region 自定义值测试

    [Fact]
    public void CustomValues_ArePreserved()
    {
        var options = new GenesisBuildOptions
        {
            ProjectRoot = "/project",
            ContentRoot = "/project/content",
            OutputRoot = "/project/build",
            CacheDirectory = "/project/.cache",
            TargetPlatform = PlatformType.Linux,
            EnableIncrementalBuild = false,
            EnableCompression = false,
            EnableEncryption = true,
            StripDebugInfo = false,
            MaxDegreeOfParallelism = 8,
            FailOnFirstError = true
        };

        Assert.Equal("/project", options.ProjectRoot);
        Assert.Equal("/project/content", options.ContentRoot);
        Assert.Equal("/project/build", options.OutputRoot);
        Assert.Equal("/project/.cache", options.CacheDirectory);
        Assert.Equal(PlatformType.Linux, options.TargetPlatform);
        Assert.False(options.EnableIncrementalBuild);
        Assert.False(options.EnableCompression);
        Assert.True(options.EnableEncryption);
        Assert.False(options.StripDebugInfo);
        Assert.Equal(8, options.MaxDegreeOfParallelism);
        Assert.True(options.FailOnFirstError);
    }

    [Fact]
    public void AllPlatformTypes_CanBeSet()
    {
        var platforms = new[]
        {
            PlatformType.Windows,
            PlatformType.Linux,
            PlatformType.macOS,
            PlatformType.Android,
            PlatformType.WebAssembly
        };

        foreach (var platform in platforms)
        {
            var options = new GenesisBuildOptions { TargetPlatform = platform };
            Assert.Equal(platform, options.TargetPlatform);
        }
    }

    #endregion
}

public class GenesisBuildResultTests
{
    #region 成功/失败判定测试

    [Fact]
    public void IsSuccess_NoFailures_ReturnsTrue()
    {
        var result = new GenesisBuildResult
        {
            TotalAssets = 10,
            SucceededCount = 10,
            FailedCount = 0,
            UpToDateCount = 0,
            SkippedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(5),
            FailedAssets = [],
            OutputBundles = ["/build/output.bundle"]
        };

        Assert.True(result.IsSuccess);
        Assert.False(result.HasErrors);
    }

    [Fact]
    public void IsSuccess_WithFailures_ReturnsFalse()
    {
        var result = new GenesisBuildResult
        {
            TotalAssets = 10,
            SucceededCount = 8,
            FailedCount = 2,
            UpToDateCount = 0,
            SkippedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(5),
            FailedAssets = ["asset1.script", "asset2.shader"],
            OutputBundles = []
        };

        Assert.False(result.IsSuccess);
        Assert.True(result.HasErrors);
    }

    [Fact]
    public void HasErrors_WithFailedAssets_ReturnsTrue()
    {
        var result = new GenesisBuildResult
        {
            TotalAssets = 5,
            SucceededCount = 4,
            FailedCount = 1,
            UpToDateCount = 0,
            SkippedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(3),
            FailedAssets = ["broken.script"],
            OutputBundles = []
        };

        Assert.True(result.HasErrors);
        Assert.Single(result.FailedAssets);
    }

    #endregion

    #region 构建结果数据测试

    [Fact]
    public void OutputBundles_ContainsPaths()
    {
        var result = new GenesisBuildResult
        {
            TotalAssets = 5,
            SucceededCount = 5,
            FailedCount = 0,
            UpToDateCount = 0,
            SkippedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(2),
            FailedAssets = [],
            OutputBundles = ["/build/windows.bundle", "/build/linux.bundle"]
        };

        Assert.Equal(2, result.OutputBundles.Count);
    }

    [Fact]
    public void TotalDuration_IsRecorded()
    {
        var duration = TimeSpan.FromMilliseconds(1234);
        var result = new GenesisBuildResult
        {
            TotalAssets = 1,
            SucceededCount = 1,
            FailedCount = 0,
            UpToDateCount = 0,
            SkippedCount = 0,
            TotalDuration = duration,
            FailedAssets = [],
            OutputBundles = []
        };

        Assert.Equal(duration, result.TotalDuration);
    }

    #endregion
}

public class GenesisBuildPipelineTests
{
    #region 构造函数测试

    [Fact]
    public void Constructor_NullFormatRegistry_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new GenesisBuildPipeline(null!));
    }

    #endregion

    #region 生命周期测试

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var registry = new FormatRegistry();
        var pipeline = new GenesisBuildPipeline(registry);

        pipeline.Dispose();
        pipeline.Dispose();
    }

    [Fact]
    public async Task BuildAsync_WithoutInitialize_ThrowsInvalidOperationException()
    {
        var registry = new FormatRegistry();
        var pipeline = new GenesisBuildPipeline(registry);
        var options = new GenesisBuildOptions
        {
            ProjectRoot = "/test",
            ContentRoot = "/test/content",
            OutputRoot = "/test/build",
            CacheDirectory = "/test/.cache"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline.BuildAsync(options));

        pipeline.Dispose();
    }

    #endregion
}

public class GenesisProjectBuilderTests
{
    #region 构造函数测试

    [Fact]
    public void Constructor_NullFormatRegistry_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new GenesisProjectBuilder(null!));
    }

    #endregion
}
