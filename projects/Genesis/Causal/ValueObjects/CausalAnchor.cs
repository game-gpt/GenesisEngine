using Genesis.Core.ValueObjects;

namespace Genesis.Causal.ValueObjects;

public readonly record struct CausalAnchor(
    string Id,
    string Name,
    double Weight,
    Dictionary<string, double> Effects)
{
    public CausalAnchor ApplyDelta(double delta) => this with { Weight = Weight + delta };
}
