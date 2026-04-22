using Genesis.Core.ValueObjects;

namespace Genesis.Causal.Interfaces;

public interface ICausalAnchor
{
    string Id { get; }
    string Name { get; }
    double Weight { get; }
    IReadOnlyDictionary<string, double> Effects { get; }
    void ApplyWeight(double delta);
}
