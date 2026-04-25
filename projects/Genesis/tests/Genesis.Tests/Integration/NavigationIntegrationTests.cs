using Genesis.Integration;
using Gnosis.Navigation.NavMesh;
using Xunit;

namespace Genesis.Tests.Integration;

public class NavigationIntegrationTests
{
    [Fact]
    public void Initialize_SetsIsInitialized()
    {
        using var integration = new NavigationIntegration();

        integration.Initialize();

        Assert.True(integration.IsInitialized);
    }

    [Fact]
    public void Initialize_WithNull_CreatesDefaultSystem()
    {
        using var integration = new NavigationIntegration();

        integration.Initialize(null);

        Assert.True(integration.IsInitialized);
    }

    [Fact]
    public void BuildNavMesh_BeforeInitialize_ThrowsInvalidOperationException()
    {
        using var integration = new NavigationIntegration();

        Assert.Throws<InvalidOperationException>(() =>
            integration.BuildNavMesh("test", new NavMeshBuildSettings()));
    }

    [Fact]
    public void FindPath_BeforeInitialize_ThrowsInvalidOperationException()
    {
        using var integration = new NavigationIntegration();

        Assert.Throws<InvalidOperationException>(() =>
            integration.FindPath(System.Numerics.Vector3.Zero, System.Numerics.Vector3.One));
    }

    [Fact]
    public void CreateQuery_BeforeInitialize_ThrowsInvalidOperationException()
    {
        using var integration = new NavigationIntegration();

        Assert.Throws<InvalidOperationException>(() => integration.CreateQuery("test"));
    }

    [Fact]
    public void IsInitialized_DefaultFalse()
    {
        using var integration = new NavigationIntegration();

        Assert.False(integration.IsInitialized);
    }

    [Fact]
    public void GetNavMesh_NonExistent_ReturnsNull()
    {
        using var integration = new NavigationIntegration();
        integration.Initialize();

        var mesh = integration.GetNavMesh("nonexistent");

        Assert.Null(mesh);
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var integration = new NavigationIntegration();
        integration.Initialize();

        integration.Dispose();
        integration.Dispose();
    }
}
