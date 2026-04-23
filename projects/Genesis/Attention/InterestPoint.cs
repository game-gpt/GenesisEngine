using Genesis.Core;

namespace Genesis.Attention;

public readonly record struct InterestPoint(
    ulong Id,
    Position Position,
    double Radius,
    double Weight,
    string Type)
{
    public bool Contains(Position position) => position.DistanceTo(Position) <= Radius;
}
