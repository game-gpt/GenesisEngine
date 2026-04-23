namespace Genesis.Causal;

public readonly record struct CausalAnchor(
    string Id,
    string Name,
    double Weight,
    Dictionary<string, double> Effects)
{
    public CausalAnchor ApplyDelta(double delta) => this with { Weight = Weight + delta };
}
