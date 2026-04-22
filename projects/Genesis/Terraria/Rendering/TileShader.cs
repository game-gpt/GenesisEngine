using Genesis.Terraria.Components;
using Genesis.Terraria.ECS;
using Gnosis.ECS;
using Gnosis.Rendering.Backends.Software;
using Gnosis.Rendering.Shader;
using Gnosis.Rendering.TwoD;

namespace Genesis.Terraria.Rendering;

public sealed class TileShader
{
    public static IShaderModule CreateTileShader()
    {
        var vertexFunc = new DelegateMicroFunction(
            "tile_vertex",
            MicroFunctionKind.Vertex,
            execute: (input, index) =>
            {
                return
                [
                    input[0], input[1], input[2], 1.0f,
                    input.Length > 3 ? input[3] : 1.0f,
                    input.Length > 4 ? input[4] : 1.0f,
                    input.Length > 5 ? input[5] : 1.0f
                ];
            }
        );

        var fragmentFunc = new DelegateMicroFunction(
            "tile_fragment",
            MicroFunctionKind.Fragment,
            execute: (interpolated, index) =>
            {
                var r = interpolated.Length > 4 ? interpolated[4] : 1.0f;
                var g = interpolated.Length > 5 ? interpolated[5] : 1.0f;
                var b = interpolated.Length > 6 ? interpolated[6] : 1.0f;

                return [r, g, b, 1.0f];
            }
        );

        return new DelegateShaderModule("tile_shader", [vertexFunc, fragmentFunc]);
    }

    public static IShaderModule CreateSpriteShader()
    {
        var vertexFunc = new DelegateMicroFunction(
            "sprite_vertex",
            MicroFunctionKind.Vertex,
            execute: (input, index) =>
            {
                return
                [
                    input[0], input[1], input[2], 1.0f,
                    input.Length > 3 ? input[3] : 1.0f,
                    input.Length > 4 ? input[4] : 1.0f,
                    input.Length > 5 ? input[5] : 1.0f
                ];
            }
        );

        var fragmentFunc = new DelegateMicroFunction(
            "sprite_fragment",
            MicroFunctionKind.Fragment,
            execute: (interpolated, index) =>
            {
                var r = interpolated.Length > 4 ? interpolated[4] : 1.0f;
                var g = interpolated.Length > 5 ? interpolated[5] : 1.0f;
                var b = interpolated.Length > 6 ? interpolated[6] : 1.0f;

                return [r, g, b, 1.0f];
            }
        );

        return new DelegateShaderModule("sprite_shader", [vertexFunc, fragmentFunc]);
    }
}
