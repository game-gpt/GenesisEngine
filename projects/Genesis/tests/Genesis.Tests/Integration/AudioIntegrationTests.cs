using Genesis.Integration.Audio2D;
using Xunit;

namespace Genesis.Tests.Integration;

public class AudioIntegrationTests
{
    [Fact]
    public void Initialize_SetsIsInitialized()
    {
        var system = new Genesis2DAudioSystem();

        system.Initialize();

        Assert.True(system.IsInitialized);
    }

    [Fact]
    public void Shutdown_SetsIsInitializedFalse()
    {
        var system = new Genesis2DAudioSystem();
        system.Initialize();

        system.Shutdown();

        Assert.False(system.IsInitialized);
    }

    [Fact]
    public void IsInitialized_DefaultFalse()
    {
        var system = new Genesis2DAudioSystem();

        Assert.False(system.IsInitialized);
    }

    [Fact]
    public void Shutdown_CalledTwice_DoesNotThrow()
    {
        var system = new Genesis2DAudioSystem();
        system.Initialize();

        system.Shutdown();
        system.Shutdown();
    }

    [Fact]
    public void GlobalVolume_DefaultIsOne()
    {
        var system = new Genesis2DAudioSystem();
        system.Initialize();

        Assert.Equal(1f, system.GlobalVolume, 0.01f);
    }

    [Fact]
    public void GlobalVolume_Setter_UpdatesValue()
    {
        var system = new Genesis2DAudioSystem();
        system.Initialize();

        system.GlobalVolume = 0.5f;

        Assert.Equal(0.5f, system.GlobalVolume, 0.01f);
    }
}
