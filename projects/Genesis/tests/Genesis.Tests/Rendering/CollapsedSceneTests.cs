using Genesis.Core;
using Genesis.Rendering;
using Genesis.Spacetime;
using Xunit;

namespace Genesis.Tests.Rendering;

public class CollapsedSceneTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var entities = new List<IRenderableEntity>();
        var features = new CausalFeatures(new double[] { 0.5, 0.3 });
        var timestamp = Timestamp.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        var scene = new CollapsedScene(entities, features, timestamp, 12345UL);

        Assert.Empty(scene.Entities);
        Assert.Equal(12345UL, scene.SceneHash);
        Assert.Equal(2, scene.GlobalFeatures.Dimension);
    }

    [Fact]
    public void ToDescription_WithEntities_ReturnsValidDescription()
    {
        var entity = new RenderableEntity(
            new EntityId(1, 0),
            "Test",
            new Position(10, 20, 30),
            new CausalFeatures(new double[] { 1.0 }));

        var scene = new CollapsedScene(
            new List<IRenderableEntity> { entity },
            CausalFeatures.Zero(4),
            Timestamp.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            99999UL);

        var desc = scene.ToDescription();
        Assert.Equal(99999UL, desc.SceneHash);
        Assert.Equal(1, desc.EntityCount);
        Assert.True(desc.IsValid);
    }

    [Fact]
    public void ToDescription_EmptyScene_ReturnsZeroBounds()
    {
        var scene = new CollapsedScene(
            new List<IRenderableEntity>(),
            CausalFeatures.Zero(4),
            Timestamp.FromUnixTimeSeconds(0),
            0UL);

        var desc = scene.ToDescription();
        Assert.Equal(0, desc.EntityCount);
    }
}

public class RenderableEntityTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var id = new EntityId(42, 1);
        var pos = new Position(1, 2, 3);
        var features = new CausalFeatures(new double[] { 0.5, 0.8 });
        var props = new Dictionary<string, object> { { "key", "value" } };

        var entity = new RenderableEntity(id, "TestType", pos, features, props);

        Assert.Equal(id, entity.Id);
        Assert.Equal("TestType", entity.Type);
        Assert.Equal(pos, entity.Position);
        Assert.Equal(2, entity.Features.Dimension);
        Assert.Equal("value", entity.Properties["key"]);
    }

    [Fact]
    public void Constructor_NullProperties_DefaultsToEmpty()
    {
        var entity = new RenderableEntity(
            new EntityId(1, 0),
            "T",
            Position.Zero,
            CausalFeatures.Zero(1));

        Assert.Empty(entity.Properties);
    }
}
