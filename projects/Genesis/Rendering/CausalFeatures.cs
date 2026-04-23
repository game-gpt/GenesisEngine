namespace Genesis.Rendering;

public readonly record struct CausalFeatures(double[] Values)
{
    public int Dimension => Values.Length;
    public double this[int index] => Values[index];
    public static CausalFeatures Zero(int dimension) => new(new double[dimension]);
    public CausalFeatures Normalize()
    {
        var length = Math.Sqrt(Values.Sum(v => v * v));
        if (length < 1e-10) return this;
        return new CausalFeatures(Values.Select(v => v / length).ToArray());
    }
}
