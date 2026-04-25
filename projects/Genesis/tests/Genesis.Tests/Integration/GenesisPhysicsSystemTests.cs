using Genesis.Integration.Physics;
using Xunit;

namespace Genesis.Tests.Integration;

public class GenesisPhysicsSystemTests
{
    [Fact]
    public void Phase_IsUpdate()
    {
        var system = new GenesisPhysicsSystem();

        Assert.Equal(Gnosis.ECS.System.SystemPhase.Update, system.Phase);
    }

    [Fact]
    public void Initialize_Succeeds()
    {
        var system = new GenesisPhysicsSystem();

        system.Initialize();
    }

    [Fact]
    public void Shutdown_Succeeds()
    {
        var system = new GenesisPhysicsSystem();
        system.Initialize();

        system.Shutdown();
    }

    [Fact]
    public void Update_WithoutWorld_DoesNotThrow()
    {
        var system = new GenesisPhysicsSystem();
        system.Initialize();

        system.Update(0.016f);
    }

    [Fact]
    public void SetWorld_DoesNotThrow()
    {
        var system = new GenesisPhysicsSystem();

        system.SetWorld(null);
    }
}
