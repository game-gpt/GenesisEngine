namespace Genesis.Core;

public readonly record struct Position(double X, double Y, double Z)
{
    public static Position Zero { get; } = new(0, 0, 0);

    public double DistanceTo(Position other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        var dz = Z - other.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public double DistanceToSquared(Position other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        var dz = Z - other.Z;
        return dx * dx + dy * dy + dz * dz;
    }
}
