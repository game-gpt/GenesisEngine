using Genesis.Rendering;
using Xunit;

namespace Genesis.Tests.Rendering;

public class CausalFeaturesTests
{
    [Fact]
    public void Dimension_ReturnsArrayLength()
    {
        var features = new CausalFeatures([1.0, 2.0, 3.0]);

        Assert.Equal(3, features.Dimension);
    }

    [Fact]
    public void Indexer_ReturnsCorrectValue()
    {
        var features = new CausalFeatures([10.0, 20.0, 30.0]);

        Assert.Equal(20.0, features[1]);
    }

    [Fact]
    public void Zero_CreatesZeroVector()
    {
        var features = CausalFeatures.Zero(4);

        Assert.Equal(4, features.Dimension);
        Assert.All(features.Values, v => Assert.Equal(0.0, v));
    }

    [Fact]
    public void Normalize_UnitVector_ReturnsSameVector()
    {
        var features = new CausalFeatures([1.0, 0.0, 0.0]);

        var normalized = features.Normalize();

        Assert.Equal(1.0, normalized[0], 10);
        Assert.Equal(0.0, normalized[1], 10);
        Assert.Equal(0.0, normalized[2], 10);
    }

    [Fact]
    public void Normalize_NonUnitVector_ReturnsUnitVector()
    {
        var features = new CausalFeatures([3.0, 4.0]);

        var normalized = features.Normalize();

        var length = Math.Sqrt(normalized.Values.Sum(v => v * v));
        Assert.Equal(1.0, length, 10);
    }

    [Fact]
    public void Normalize_ZeroVector_ReturnsSameVector()
    {
        var features = CausalFeatures.Zero(3);

        var normalized = features.Normalize();

        Assert.Equal(features, normalized);
    }

    [Fact]
    public void Normalize_DoesNotModifyOriginal()
    {
        var features = new CausalFeatures([3.0, 4.0]);

        features.Normalize();

        Assert.Equal(3.0, features[0]);
        Assert.Equal(4.0, features[1]);
    }
}
