namespace Genesis.Core.ValueObjects;

public readonly record struct Bounds(double MinX, double MinY, double MinZ, double MaxX, double MaxY, double MaxZ)
{
    public double SizeX => MaxX - MinX;
    public double SizeY => MaxY - MinY;
    public double SizeZ => MaxZ - MinZ;
    public Position Center => new((MinX + MaxX) / 2, (MinY + MaxY) / 2, (MinZ + MaxZ) / 2);
    public bool Contains(Position position) => 
        position.X >= MinX && position.X <= MaxX &&
        position.Y >= MinY && position.Y <= MaxY &&
        position.Z >= MinZ && position.Z <= MaxZ;
}
