using Genesis.Core;

namespace Genesis.Attention;

public sealed class SimpleAttentionModel : IAttentionModel
{
    #region 字段

    private const double MaxDistance = 1000.0;
    private const double ViewAngleThreshold = 0.5;

    #endregion

    #region IAttentionModel 实现

    public double CalculateDistanceFactor(Position position, Position playerPosition)
    {
        var distance = position.DistanceTo(playerPosition);
        if (distance >= MaxDistance) return 0.0;
        return 1.0 - (distance / MaxDistance);
    }

    public double CalculateViewFactor(Position position, Position playerPosition, Position viewDirection)
    {
        var toObject = new Position(
            position.X - playerPosition.X,
            position.Y - playerPosition.Y,
            position.Z - playerPosition.Z);

        var distance = toObject.DistanceTo(Position.Zero);
        if (distance < 1e-10) return 1.0;

        var normalized = new Position(
            toObject.X / distance,
            toObject.Y / distance,
            toObject.Z / distance);

        var dot = normalized.X * viewDirection.X +
                  normalized.Y * viewDirection.Y +
                  normalized.Z * viewDirection.Z;

        return dot > ViewAngleThreshold ? dot : 0.0;
    }

    public double CalculateInteractionFactor(ulong entityId)
    {
        return 0.0;
    }

    public double CalculateCausalImportance(ulong historyHash)
    {
        return historyHash != 0 ? 0.1 : 0.0;
    }

    public double CombineFactors(double distance, double view, double interaction, double causal)
    {
        return 0.5 * distance + 0.3 * view + 0.1 * interaction + 0.1 * causal;
    }

    #endregion
}
