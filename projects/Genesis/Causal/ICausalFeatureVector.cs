namespace Genesis.Causal.Interfaces;

public interface ICausalFeatureVector
{
    int Dimension { get; }
    double this[int index] { get; set; }
    double[] ToArray();
    void Normalize();
    double Dot(ICausalFeatureVector other);
}
