using Genesis.Integration.Navigation2D;
using Gnosis.Navigation.NavMesh;
using Xunit;

namespace Genesis.Tests.Integration;

public class NavigationIntegrationTests
{
    [Fact]
    public void Initialize_SetsIsInitialized()
    {
        var system = new Genesis2DNavigationSystem();

        system.Initialize();

        Assert.True(system.IsInitialized);
    }

    [Fact]
    public void Shutdown_SetsIsInitializedFalse()
    {
        var system = new Genesis2DNavigationSystem();
        system.Initialize();

        system.Shutdown();

        Assert.False(system.IsInitialized);
    }

    [Fact]
    public void IsInitialized_DefaultFalse()
    {
        var system = new Genesis2DNavigationSystem();

        Assert.False(system.IsInitialized);
    }

    [Fact]
    public void GetNavMesh_NonExistent_ReturnsNull()
    {
        var system = new Genesis2DNavigationSystem();
        system.Initialize();

        var mesh = system.GetNavMesh("nonexistent");

        Assert.Null(mesh);
    }

    [Fact]
    public void Shutdown_CalledTwice_DoesNotThrow()
    {
        var system = new Genesis2DNavigationSystem();
        system.Initialize();

        system.Shutdown();
        system.Shutdown();
    }
}
