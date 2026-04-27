using Genesis.Integration.AI2D;
using Xunit;

namespace Genesis.Tests.Integration;

public class AIIntegrationTests
{
    [Fact]
    public void Initialize_SetsIsInitialized()
    {
        var system = new Genesis2DAISystem();

        system.Initialize();

        Assert.True(system.IsInitialized);
    }

    [Fact]
    public void Shutdown_SetsIsInitializedFalse()
    {
        var system = new Genesis2DAISystem();
        system.Initialize();

        system.Shutdown();

        Assert.False(system.IsInitialized);
    }

    [Fact]
    public void CreateBehaviorTree_BeforeInitialize_ThrowsInvalidOperationException()
    {
        var system = new Genesis2DAISystem();

        Assert.Throws<InvalidOperationException>(() => system.CreateBehaviorTree("test"));
    }

    [Fact]
    public void CreateController_BeforeInitialize_ThrowsInvalidOperationException()
    {
        var system = new Genesis2DAISystem();

        Assert.Throws<InvalidOperationException>(() => system.CreateController("test"));
    }

    [Fact]
    public void IsInitialized_DefaultFalse()
    {
        var system = new Genesis2DAISystem();

        Assert.False(system.IsInitialized);
    }

    [Fact]
    public void GetBehaviorTree_NonExistent_ReturnsNull()
    {
        var system = new Genesis2DAISystem();
        system.Initialize();

        var tree = system.GetBehaviorTree("nonexistent");

        Assert.Null(tree);
    }

    [Fact]
    public void Shutdown_CalledTwice_DoesNotThrow()
    {
        var system = new Genesis2DAISystem();
        system.Initialize();

        system.Shutdown();
        system.Shutdown();
    }

    [Fact]
    public void StartBehaviorTree_NonExistent_DoesNotThrow()
    {
        var system = new Genesis2DAISystem();
        system.Initialize();

        system.StartBehaviorTree("nonexistent");
    }

    [Fact]
    public void StopBehaviorTree_NonExistent_DoesNotThrow()
    {
        var system = new Genesis2DAISystem();
        system.Initialize();

        system.StopBehaviorTree("nonexistent");
    }
}
