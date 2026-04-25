using Genesis.Integration;
using Xunit;

namespace Genesis.Tests.Integration;

public class AudioIntegrationTests
{
    [Fact]
    public void Initialize_SetsIsInitialized()
    {
        using var integration = new AudioIntegration();

        integration.Initialize();

        Assert.True(integration.IsInitialized);
    }

    [Fact]
    public void Initialize_WithNull_CreatesDefaultSystem()
    {
        using var integration = new AudioIntegration();

        integration.Initialize(null);

        Assert.True(integration.IsInitialized);
    }

    [Fact]
    public void CreateSource_BeforeInitialize_ThrowsInvalidOperationException()
    {
        using var integration = new AudioIntegration();

        Assert.Throws<InvalidOperationException>(() => integration.CreateSource("test"));
    }

    [Fact]
    public void LoadClip_BeforeInitialize_ThrowsInvalidOperationException()
    {
        using var integration = new AudioIntegration();

        Assert.Throws<InvalidOperationException>(() => integration.LoadClip("test", "path.wav"));
    }

    [Fact]
    public void CreateBus_BeforeInitialize_ThrowsInvalidOperationException()
    {
        using var integration = new AudioIntegration();

        Assert.Throws<InvalidOperationException>(() => integration.CreateBus("test"));
    }

    [Fact]
    public void IsInitialized_DefaultFalse()
    {
        using var integration = new AudioIntegration();

        Assert.False(integration.IsInitialized);
    }

    [Fact]
    public void MasterBus_BeforeInitialize_Null()
    {
        using var integration = new AudioIntegration();

        Assert.Null(integration.MasterBus);
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var integration = new AudioIntegration();
        integration.Initialize();

        integration.Dispose();
        integration.Dispose();
    }

    [Fact]
    public void GetSource_NonExistent_ReturnsNull()
    {
        using var integration = new AudioIntegration();
        integration.Initialize();

        var source = integration.GetSource("nonexistent");

        Assert.Null(source);
    }
}
