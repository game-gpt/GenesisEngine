using Genesis.Integration;
using Xunit;

namespace Genesis.Tests.Integration;

public class AIIntegrationTests
{
    [Fact]
    public void Initialize_SetsIsInitialized()
    {
        using var integration = new AIIntegration();

        integration.Initialize();

        Assert.True(integration.IsInitialized);
    }

    [Fact]
    public void Initialize_WithNull_CreatesDefaultSystem()
    {
        using var integration = new AIIntegration();

        integration.Initialize(null);

        Assert.True(integration.IsInitialized);
    }

    [Fact]
    public void CreateBehaviorTree_BeforeInitialize_ThrowsInvalidOperationException()
    {
        using var integration = new AIIntegration();

        Assert.Throws<InvalidOperationException>(() => integration.CreateBehaviorTree("test"));
    }

    [Fact]
    public void CreateController_BeforeInitialize_ThrowsInvalidOperationException()
    {
        using var integration = new AIIntegration();

        Assert.Throws<InvalidOperationException>(() => integration.CreateController("test"));
    }

    [Fact]
    public void IsInitialized_DefaultFalse()
    {
        using var integration = new AIIntegration();

        Assert.False(integration.IsInitialized);
    }

    [Fact]
    public void GetBehaviorTree_NonExistent_ReturnsNull()
    {
        using var integration = new AIIntegration();
        integration.Initialize();

        var tree = integration.GetBehaviorTree("nonexistent");

        Assert.Null(tree);
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var integration = new AIIntegration();
        integration.Initialize();

        integration.Dispose();
        integration.Dispose();
    }

    [Fact]
    public void StartBehaviorTree_NonExistent_DoesNotThrow()
    {
        using var integration = new AIIntegration();
        integration.Initialize();

        integration.StartBehaviorTree("nonexistent");
    }

    [Fact]
    public void StopBehaviorTree_NonExistent_DoesNotThrow()
    {
        using var integration = new AIIntegration();
        integration.Initialize();

        integration.StopBehaviorTree("nonexistent");
    }
}
