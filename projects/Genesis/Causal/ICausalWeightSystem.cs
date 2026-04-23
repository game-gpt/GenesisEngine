using Genesis.Core;

namespace Genesis.Causal;

public interface ICausalWeightSystem
{
    void AddAnchor(PlayerId playerId, ICausalAnchor anchor);
    void RemoveAnchor(PlayerId playerId, string anchorId);
    double GetTotalWeight(PlayerId playerId, string dimension);
    IReadOnlyDictionary<string, double> GetAllWeights(PlayerId playerId);
    void PropagateWeights(PlayerId sourcePlayer, PlayerId targetPlayer, double factor);
}
