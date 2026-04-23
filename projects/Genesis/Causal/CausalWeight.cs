namespace Genesis.Causal;

public readonly record struct CausalWeight(string Dimension, double Value)
{
    public CausalWeight Add(double delta) => new(Dimension, Value + delta);
    public CausalWeight Clamp(double min, double max) => new(Dimension, Math.Clamp(Value, min, max));
}
