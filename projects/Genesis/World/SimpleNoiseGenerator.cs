using Genesis.Core;

namespace Genesis.World;

public sealed class SimpleNoiseGenerator : INoiseGenerator
{
    #region 字段

    private int _octaves;
    private double _persistence;
    private double _lacunarity;
    private readonly int[] _perm;
    private readonly double[] _gradientsX;
    private readonly double[] _gradientsY;
    private readonly double[] _gradientsZ;

    #endregion

    #region 构造函数

    public SimpleNoiseGenerator(ulong seed = 0)
    {
        _octaves = 4;
        _persistence = 0.5;
        _lacunarity = 2.0;

        var perm = new int[512];
        var gradientsX = new double[256];
        var gradientsY = new double[256];
        var gradientsZ = new double[256];

        var rng = seed == 0 ? new Random() : new Random((int)seed);

        for (var i = 0; i < 256; i++)
        {
            perm[i] = i;
        }

        for (var i = 255; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (perm[i], perm[j]) = (perm[j], perm[i]);
        }

        for (var i = 0; i < 256; i++)
        {
            perm[i + 256] = perm[i];
            var angle = rng.NextDouble() * Math.PI * 2;
            gradientsX[i] = Math.Cos(angle);
            gradientsY[i] = Math.Sin(angle);

            var theta = rng.NextDouble() * Math.PI * 2;
            var phi = rng.NextDouble() * Math.PI;
            gradientsZ[i] = Math.Sin(phi) * Math.Cos(theta);
        }

        _perm = perm;
        _gradientsX = gradientsX;
        _gradientsY = gradientsY;
        _gradientsZ = gradientsZ;
    }

    #endregion

    #region INoiseGenerator 实现

    public double Noise2D(double x, double y)
    {
        double total = 0;
        double amplitude = 1.0;
        double frequency = 1.0;
        double maxValue = 0;

        for (var i = 0; i < _octaves; i++)
        {
            total += Perlin2D(x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= _persistence;
            frequency *= _lacunarity;
        }

        return total / maxValue;
    }

    public double Noise3D(double x, double y, double z)
    {
        double total = 0;
        double amplitude = 1.0;
        double frequency = 1.0;
        double maxValue = 0;

        for (var i = 0; i < _octaves; i++)
        {
            total += Perlin3D(x * frequency, y * frequency, z * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= _persistence;
            frequency *= _lacunarity;
        }

        return total / maxValue;
    }

    public double[] GenerateNoiseMap(Bounds bounds, int resolution, ulong seed)
    {
        var noise = new SimpleNoiseGenerator(seed);
        noise.SetOctaves(_octaves);
        noise.SetPersistence(_persistence);
        noise.SetLacunarity(_lacunarity);

        var result = new double[resolution * resolution];
        var stepX = bounds.SizeX / resolution;
        var stepY = bounds.SizeY / resolution;

        for (var y = 0; y < resolution; y++)
        {
            for (var x = 0; x < resolution; x++)
            {
                var nx = bounds.MinX + x * stepX;
                var ny = bounds.MinY + y * stepY;
                result[y * resolution + x] = noise.Noise2D(nx, ny);
            }
        }

        return result;
    }

    public void SetOctaves(int octaves)
    {
        _octaves = Math.Clamp(octaves, 1, 16);
    }

    public void SetPersistence(double persistence)
    {
        _persistence = Math.Clamp(persistence, 0.0, 1.0);
    }

    public void SetLacunarity(double lacunarity)
    {
        _lacunarity = Math.Max(1.0, lacunarity);
    }

    #endregion

    #region Perlin 噪声核心

    private double Perlin2D(double x, double y)
    {
        var xi = (int)Math.Floor(x);
        var yi = (int)Math.Floor(y);
        var xf = x - xi;
        var yf = y - yi;

        var u = Fade(xf);
        var v = Fade(yf);

        var aa = _perm[_perm[xi & 255] + (yi & 255)];
        var ab = _perm[_perm[xi & 255] + ((yi + 1) & 255)];
        var ba = _perm[_perm[(xi + 1) & 255] + (yi & 255)];
        var bb = _perm[_perm[(xi + 1) & 255] + ((yi + 1) & 255)];

        var x1 = Lerp(Dot2D(aa, xf, yf), Dot2D(ba, xf - 1, yf), u);
        var x2 = Lerp(Dot2D(ab, xf, yf - 1), Dot2D(bb, xf - 1, yf - 1), u);

        return Lerp(x1, x2, v);
    }

    private double Perlin3D(double x, double y, double z)
    {
        var xi = (int)Math.Floor(x);
        var yi = (int)Math.Floor(y);
        var zi = (int)Math.Floor(z);
        var xf = x - xi;
        var yf = y - yi;
        var zf = z - zi;

        var u = Fade(xf);
        var v = Fade(yf);
        var w = Fade(zf);

        var aaa = _perm[_perm[_perm[xi & 255] + (yi & 255)] + (zi & 255)];
        var aba = _perm[_perm[_perm[xi & 255] + ((yi + 1) & 255)] + (zi & 255)];
        var aab = _perm[_perm[_perm[xi & 255] + (yi & 255)] + ((zi + 1) & 255)];
        var abb = _perm[_perm[_perm[xi & 255] + ((yi + 1) & 255)] + ((zi + 1) & 255)];
        var baa = _perm[_perm[_perm[(xi + 1) & 255] + (yi & 255)] + (zi & 255)];
        var bba = _perm[_perm[_perm[(xi + 1) & 255] + ((yi + 1) & 255)] + (zi & 255)];
        var bab = _perm[_perm[_perm[(xi + 1) & 255] + (yi & 255)] + ((zi + 1) & 255)];
        var bbb = _perm[_perm[_perm[(xi + 1) & 255] + ((yi + 1) & 255)] + ((zi + 1) & 255)];

        var x1 = Lerp(Dot3D(aaa, xf, yf, zf), Dot3D(baa, xf - 1, yf, zf), u);
        var x2 = Lerp(Dot3D(aba, xf, yf - 1, zf), Dot3D(bba, xf - 1, yf - 1, zf), u);
        var y1 = Lerp(x1, x2, v);

        x1 = Lerp(Dot3D(aab, xf, yf, zf - 1), Dot3D(bab, xf - 1, yf, zf - 1), u);
        x2 = Lerp(Dot3D(abb, xf, yf - 1, zf - 1), Dot3D(bbb, xf - 1, yf - 1, zf - 1), u);
        var y2 = Lerp(x1, x2, v);

        return Lerp(y1, y2, w);
    }

    #endregion

    #region 辅助方法

    private static double Fade(double t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static double Lerp(double a, double b, double t)
    {
        return a + t * (b - a);
    }

    private double Dot2D(int hash, double x, double y)
    {
        var idx = hash & 255;
        return _gradientsX[idx] * x + _gradientsY[idx] * y;
    }

    private double Dot3D(int hash, double x, double y, double z)
    {
        var idx = hash & 255;
        return _gradientsX[idx] * x + _gradientsY[idx] * y + _gradientsZ[idx] * z;
    }

    #endregion
}
