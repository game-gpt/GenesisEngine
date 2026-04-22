using Genesis.Terraria.Components;
using Genesis.Terraria.ECS;
using Gnosis.ECS;
using Gnosis.Rendering.Backends.Software;
using Gnosis.Rendering.Shader;
using Gnosis.Rendering.TwoD;

namespace Genesis.Terraria.Rendering;

public sealed class Camera2D : ICamera2D
{
    public float[] Position { get; set; } = [0, 0];
    public float Zoom { get; set; } = 1.0f;
    public float Rotation { get; set; } = 0.0f;
    public float[] ViewportSize { get; set; } = [640, 480];

    public float[] ScreenToWorld(float[] screenPoint)
    {
        return
        [
            (screenPoint[0] - ViewportSize[0] / 2) / Zoom + Position[0],
            (screenPoint[1] - ViewportSize[1] / 2) / Zoom + Position[1]
        ];
    }

    public float[] WorldToScreen(float[] worldPoint)
    {
        return
        [
            (worldPoint[0] - Position[0]) * Zoom + ViewportSize[0] / 2,
            (worldPoint[1] - Position[1]) * Zoom + ViewportSize[1] / 2
        ];
    }
}
