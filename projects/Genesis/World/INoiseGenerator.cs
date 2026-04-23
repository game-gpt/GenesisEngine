using Genesis.Core;

namespace Genesis.World;

public interface INoiseGenerator
{
    double Noise2D(double x, double y);
    double Noise3D(double x, double y, double z);
    double[] GenerateNoiseMap(Bounds bounds, int resolution, ulong seed);
    void SetOctaves(int octaves);
    void SetPersistence(double persistence);
    void SetLacunarity(double lacunarity);
}
