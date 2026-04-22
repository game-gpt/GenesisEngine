namespace Genesis.Terraria.World;

public sealed class SimplexNoise
{
    private readonly int[] _perm;

    private static readonly int[][] Grad3 =
    [
        [1, 1, 0], [-1, 1, 0], [1, -1, 0], [-1, -1, 0],
        [1, 0, 1], [-1, 0, 1], [1, 0, -1], [-1, 0, -1],
        [0, 1, 1], [0, -1, 1], [0, 1, -1], [0, -1, -1]
    ];

    public SimplexNoise(ulong seed)
    {
        _perm = new int[512];
        var p = new int[256];

        for (var i = 0; i < 256; i++)
        {
            p[i] = i;
        }

        var rng = new Random((int)(seed ^ (seed >> 32)));

        for (var i = 255; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (p[i], p[j]) = (p[j], p[i]);
        }

        for (var i = 0; i < 512; i++)
        {
            _perm[i] = p[i & 255];
        }
    }

    private static double Dot(int[] g, double x, double y)
    {
        return g[0] * x + g[1] * y;
    }

    private static readonly double F2 = 0.5 * (Math.Sqrt(3.0) - 1.0);
    private static readonly double G2 = (3.0 - Math.Sqrt(3.0)) / 6.0;

    public double Noise2D(double x, double y)
    {
        var s = (x + y) * F2;
        var i = (int)Math.Floor(x + s);
        var j = (int)Math.Floor(y + s);

        var t = (i + j) * G2;
        var x0 = x - (i - t);
        var y0 = y - (j - t);

        int i1;
        int j1;

        if (x0 > y0)
        {
            i1 = 1;
            j1 = 0;
        }
        else
        {
            i1 = 0;
            j1 = 1;
        }

        var x1 = x0 - i1 + G2;
        var y1 = y0 - j1 + G2;
        var x2 = x0 - 1.0 + 2.0 * G2;
        var y2 = y0 - 1.0 + 2.0 * G2;

        var ii = i & 255;
        var jj = j & 255;

        var n0 = 0.0;
        var n1 = 0.0;
        var n2 = 0.0;

        var t0 = 0.5 - x0 * x0 - y0 * y0;
        if (t0 >= 0)
        {
            var gi0 = _perm[ii + _perm[jj]] % 12;
            t0 *= t0;
            n0 = t0 * t0 * Dot(Grad3[gi0], x0, y0);
        }

        var t1 = 0.5 - x1 * x1 - y1 * y1;
        if (t1 >= 0)
        {
            var gi1 = _perm[ii + i1 + _perm[jj + j1]] % 12;
            t1 *= t1;
            n1 = t1 * t1 * Dot(Grad3[gi1], x1, y1);
        }

        var t2 = 0.5 - x2 * x2 - y2 * y2;
        if (t2 >= 0)
        {
            var gi2 = _perm[ii + 1 + _perm[jj + 1]] % 12;
            t2 *= t2;
            n2 = t2 * t2 * Dot(Grad3[gi2], x2, y2);
        }

        return 70.0 * (n0 + n1 + n2);
    }

    public double FractalNoise2D(double x, double y, int octaves, double persistence = 0.5, double lacunarity = 2.0)
    {
        var total = 0.0;
        var frequency = 1.0;
        var amplitude = 1.0;
        var maxValue = 0.0;

        for (var i = 0; i < octaves; i++)
        {
            total += Noise2D(x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / maxValue;
    }
}
