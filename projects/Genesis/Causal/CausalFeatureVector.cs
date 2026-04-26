namespace Genesis.Causal;

public sealed class CausalFeatureVector : ICausalFeatureVector
{
    #region 字段

    private readonly double[] _values;

    #endregion

    #region 属性

    public int Dimension => _values.Length;

    public double this[int index]
    {
        get
        {
            if (index < 0 || index >= _values.Length)
            {
                throw new IndexOutOfRangeException($"索引超出范围: {index}");
            }
            return _values[index];
        }
        set
        {
            if (index < 0 || index >= _values.Length)
            {
                throw new IndexOutOfRangeException($"索引超出范围: {index}");
            }
            _values[index] = value;
        }
    }

    #endregion

    #region 构造函数

    public CausalFeatureVector(int dimension)
    {
        _values = new double[dimension];
    }

    public CausalFeatureVector(double[] values)
    {
        _values = new double[values.Length];
        Array.Copy(values, _values, values.Length);
    }

    public static CausalFeatureVector FromAnchors(IReadOnlyDictionary<AnchorType, double> anchorWeights)
    {
        var dimension = Enum.GetValues<AnchorType>().Length;
        var vector = new CausalFeatureVector(dimension);
        foreach (var (type, weight) in anchorWeights)
        {
            vector[(int)type] = weight;
        }
        return vector;
    }

    #endregion

    #region ICausalFeatureVector 实现

    public double[] ToArray()
    {
        var result = new double[_values.Length];
        Array.Copy(_values, result, _values.Length);
        return result;
    }

    public void Normalize()
    {
        var magnitude = Math.Sqrt(_values.Sum(v => v * v));
        if (magnitude < 1e-10)
        {
            return;
        }

        for (var i = 0; i < _values.Length; i++)
        {
            _values[i] /= magnitude;
        }
    }

    public double Dot(ICausalFeatureVector other)
    {
        if (other.Dimension != Dimension)
        {
            throw new ArgumentException($"维度不匹配: {other.Dimension} != {Dimension}");
        }

        var sum = 0.0;
        for (var i = 0; i < Dimension; i++)
        {
            sum += _values[i] * other[i];
        }
        return sum;
    }

    #endregion

    #region 公开方法

    public double CosineSimilarity(ICausalFeatureVector other)
    {
        var dot = Dot(other);
        var magA = Math.Sqrt(_values.Sum(v => v * v));
        var magB = Math.Sqrt(Enumerable.Range(0, other.Dimension).Sum(i => other[i] * other[i]));

        if (magA < 1e-10 || magB < 1e-10)
        {
            return 0;
        }

        return dot / (magA * magB);
    }

    public CausalFeatureVector Add(ICausalFeatureVector other)
    {
        if (other.Dimension != Dimension)
        {
            throw new ArgumentException($"维度不匹配: {other.Dimension} != {Dimension}");
        }

        var result = new CausalFeatureVector(Dimension);
        for (var i = 0; i < Dimension; i++)
        {
            result[i] = _values[i] + other[i];
        }
        return result;
    }

    public CausalFeatureVector Scale(double factor)
    {
        var result = new CausalFeatureVector(Dimension);
        for (var i = 0; i < Dimension; i++)
        {
            result[i] = _values[i] * factor;
        }
        return result;
    }

    public double Magnitude => Math.Sqrt(_values.Sum(v => v * v));

    #endregion
}
