using Genesis.Core;

namespace Genesis.Attention;

public interface IAttentionModel
{
    double CalculateDistanceFactor(Position position, Position playerPosition);
    double CalculateViewFactor(Position position, Position playerPosition, Position viewDirection);
    double CalculateInteractionFactor(ulong entityId);
    double CalculateCausalImportance(ulong historyHash);
    double CombineFactors(double distance, double view, double interaction, double causal);
}
