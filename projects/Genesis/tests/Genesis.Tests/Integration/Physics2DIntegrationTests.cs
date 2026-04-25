using Genesis.Integration;
using Xunit;

namespace Genesis.Tests.Integration;

public class Physics2DIntegrationTests
{
    [Fact]
    public void Initialize_SetsIsInitialized()
    {
        using var integration = new Physics2DIntegration();

        integration.Initialize();

        Assert.True(integration.IsInitialized);
    }

    [Fact]
    public void Dispose_SetsIsInitializedFalse()
    {
        var integration = new Physics2DIntegration();
        integration.Initialize();

        integration.Dispose();

        Assert.False(integration.IsInitialized);
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var integration = new Physics2DIntegration();
        integration.Initialize();

        integration.Dispose();
        integration.Dispose();
    }

    [Fact]
    public void IsInitialized_DefaultFalse()
    {
        using var integration = new Physics2DIntegration();

        Assert.False(integration.IsInitialized);
    }
}
