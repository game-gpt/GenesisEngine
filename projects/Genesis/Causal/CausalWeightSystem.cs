using Genesis.Core;

namespace Genesis.Causal;

public sealed class CausalWeightSystem : ICausalWeightSystem
{
    #region 字段

    private readonly Dictionary<(PlayerId, string), double> _weights = new();
    private readonly Dictionary<PlayerId, Dictionary<string, ICausalAnchor>> _anchors = new();
    private readonly Dictionary<PlayerId, CausalFeatureVector> _featureVectors = new();
    private readonly int _featureDimension;

    #endregion

    #region 属性

    public int FeatureDimension => _featureDimension;

    #endregion

    #region 构造函数

    public CausalWeightSystem(int featureDimension = 8)
    {
        _featureDimension = featureDimension;
    }

    #endregion

    #region ICausalWeightSystem 实现

    public void AddAnchor(PlayerId playerId, ICausalAnchor anchor)
    {
        if (!_anchors.ContainsKey(playerId))
        {
            _anchors[playerId] = new Dictionary<string, ICausalAnchor>();
        }

        _anchors[playerId][anchor.Id] = anchor;

        foreach (var (dimension, effect) in anchor.Effects)
        {
            var key = (playerId, dimension);
            _weights.TryGetValue(key, out var current);
            _weights[key] = current + anchor.Weight * effect;
        }

        UpdateFeatureVector(playerId);
    }

    public void RemoveAnchor(PlayerId playerId, string anchorId)
    {
        if (!_anchors.TryGetValue(playerId, out var playerAnchors))
        {
            return;
        }

        if (!playerAnchors.TryGetValue(anchorId, out var anchor))
        {
            return;
        }

        foreach (var (dimension, effect) in anchor.Effects)
        {
            var key = (playerId, dimension);
            if (_weights.TryGetValue(key, out var current))
            {
                _weights[key] = current - anchor.Weight * effect;
            }
        }

        playerAnchors.Remove(anchorId);
        UpdateFeatureVector(playerId);
    }

    public double GetTotalWeight(PlayerId playerId, string dimension)
    {
        return _weights.GetValueOrDefault((playerId, dimension), 0);
    }

    public IReadOnlyDictionary<string, double> GetAllWeights(PlayerId playerId)
    {
        var result = new Dictionary<string, double>();
        foreach (var ((pid, dim), weight) in _weights)
        {
            if (pid.Equals(playerId))
            {
                result[dim] = weight;
            }
        }
        return result;
    }

    public void PropagateWeights(PlayerId sourcePlayer, PlayerId targetPlayer, double factor)
    {
        var sourceWeights = GetAllWeights(sourcePlayer);
        foreach (var (dimension, weight) in sourceWeights)
        {
            var key = (targetPlayer, dimension);
            _weights.TryGetValue(key, out var current);
            _weights[key] = current + weight * factor;
        }

        UpdateFeatureVector(targetPlayer);
    }

    #endregion

    #region 公开方法

    public CausalFeatureVector? GetFeatureVector(PlayerId playerId)
    {
        return _featureVectors.GetValueOrDefault(playerId);
    }

    public double ComputeSimilarity(PlayerId playerA, PlayerId playerB)
    {
        var vecA = GetFeatureVector(playerA);
        var vecB = GetFeatureVector(playerB);

        if (vecA is null || vecB is null)
        {
            return 0;
        }

        return vecA.CosineSimilarity(vecB);
    }

    public IReadOnlyDictionary<string, ICausalAnchor> GetAnchors(PlayerId playerId)
    {
        return _anchors.GetValueOrDefault(playerId, new Dictionary<string, ICausalAnchor>());
    }

    public void DecayAll(double decayFactor)
    {
        var keys = _weights.Keys.ToList();
        foreach (var key in keys)
        {
            _weights[key] *= decayFactor;
        }

        foreach (var playerId in _featureVectors.Keys.ToList())
        {
            UpdateFeatureVector(playerId);
        }
    }

    #endregion

    #region 私有方法

    private void UpdateFeatureVector(PlayerId playerId)
    {
        var weights = GetAllWeights(playerId);
        var vector = new CausalFeatureVector(_featureDimension);

        foreach (var (dimension, weight) in weights)
        {
            var dimIndex = (int)Enum.Parse<AnchorType>(dimension, ignoreCase: true);
            if (dimIndex < _featureDimension)
            {
                vector[dimIndex] = weight;
            }
        }

        _featureVectors[playerId] = vector;
    }

    #endregion
}
