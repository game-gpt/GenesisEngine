using Genesis.Core.ValueObjects;

namespace Genesis.Attention.ValueObjects;

public readonly record struct InterestPoint(
    ulong Id,
    Position Position,
    double Radius,
    double Weight,
    string Type)
{
    public bool Contains(Position position) => position.DistanceTo(Position) <= Radius;
}
