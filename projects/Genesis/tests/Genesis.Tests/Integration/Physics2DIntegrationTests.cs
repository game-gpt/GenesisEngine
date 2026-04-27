using Genesis.Integration.Physics2D;
using Xunit;

namespace Genesis.Tests.Integration;

public class Physics2DIntegrationTests
{
    [Fact]
    public void Initialize_SetsIsInitialized()
    {
        var system = new Genesis2DPhysicsSystem();

        system.Initialize();

        Assert.True(system.IsInitialized);
    }

    [Fact]
    public void Shutdown_SetsIsInitializedFalse()
    {
        var system = new Genesis2DPhysicsSystem();
        system.Initialize();

        system.Shutdown();

        Assert.False(system.IsInitialized);
    }

    [Fact]
    public void Shutdown_CalledTwice_DoesNotThrow()
    {
        var system = new Genesis2DPhysicsSystem();
        system.Initialize();

        system.Shutdown();
        system.Shutdown();
    }

    [Fact]
    public void IsInitialized_DefaultFalse()
    {
        var system = new Genesis2DPhysicsSystem();

        Assert.False(system.IsInitialized);
    }

    [Fact]
    public void Gravity2D_DefaultIsNegative9_81()
    {
        var system = new Genesis2DPhysicsSystem();
        system.Initialize();

        Assert.Equal(0f, system.Gravity2D.X, 0.01f);
        Assert.Equal(-9.81f, system.Gravity2D.Y, 0.01f);
    }

    [Fact]
    public void Gravity2D_Setter_UpdatesPhysicsWorld()
    {
        var system = new Genesis2DPhysicsSystem();
        system.Initialize();

        system.Gravity2D = new System.Numerics.Vector2(1f, -5f);

        Assert.Equal(1f, system.Gravity2D.X, 0.01f);
        Assert.Equal(-5f, system.Gravity2D.Y, 0.01f);
    }

    [Fact]
    public void Raycast2D_BeforeInitialize_ThrowsInvalidOperationException()
    {
        var system = new Genesis2DPhysicsSystem();

        Assert.Throws<InvalidOperationException>(() =>
            system.Raycast2D(System.Numerics.Vector2.Zero, System.Numerics.Vector2.UnitY, 10f));
    }

    [Fact]
    public void OverlapCircle_BeforeInitialize_ThrowsInvalidOperationException()
    {
        var system = new Genesis2DPhysicsSystem();

        Assert.Throws<InvalidOperationException>(() =>
            system.OverlapCircle(System.Numerics.Vector2.Zero, 5f));
    }
}
