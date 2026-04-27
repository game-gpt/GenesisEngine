using System.Runtime.InteropServices;
using Gnosis.Graphic.UI;
using Gnosis.Platform.Window.GL;

namespace GenesisEngine.Rendering;

public sealed unsafe class GLUiRenderer : IDisposable
{
    #region 着色器源码

    private const string VertexShaderSource = @"
#version 330 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec4 aColor;
layout(location = 2) in vec2 aUv;
layout(location = 3) in vec2 aRectSize;
layout(location = 4) in vec4 aCornerRadii;
layout(location = 5) in float aDrawType;
layout(location = 6) in float aBorderWidth;
layout(location = 7) in vec4 aBorderColor;

out vec4 vColor;
out vec2 vUv;
out vec2 vRectSize;
out vec4 vCornerRadii;
out float vDrawType;
out float vBorderWidth;
out vec4 vBorderColor;

void main()
{
    gl_Position = vec4(aPosition, 1.0);
    vColor = aColor;
    vUv = aUv;
    vRectSize = aRectSize;
    vCornerRadii = aCornerRadii;
    vDrawType = aDrawType;
    vBorderWidth = aBorderWidth;
    vBorderColor = aBorderColor;
}
";

    private const string FragmentShaderSource = @"
#version 330 core

in vec4 vColor;
in vec2 vUv;
in vec2 vRectSize;
in vec4 vCornerRadii;
in float vDrawType;
in float vBorderWidth;
in vec4 vBorderColor;

out vec4 FragColor;

float sdfRoundedBox(vec2 p, vec2 b, vec4 r)
{
    r.xy = (p.x > 0.0) ? r.xy : r.zw;
    r.x  = (p.y > 0.0) ? r.x  : r.y;
    vec2 q = abs(p) - b + r.x;
    return min(max(q.x, q.y), 0.0) - length(max(q, 0.0)) + r.x;
}

void main()
{
    int drawType = int(vDrawType + 0.5);

    if (drawType == 1)
    {
        vec2 halfSize = vRectSize * 0.5;
        vec2 localPos = (vUv - 0.5) * vRectSize;
        float d = sdfRoundedBox(localPos, halfSize - vCornerRadii.x, vCornerRadii);
        float alpha = 1.0 - smoothstep(-1.0, 1.0, d);
        FragColor = vec4(vColor.rgb, vColor.a * alpha);
    }
    else if (drawType == 2)
    {
        vec2 halfSize = vRectSize * 0.5;
        vec2 localPos = (vUv - 0.5) * vRectSize;
        float d = sdfRoundedBox(localPos, halfSize - vCornerRadii.x, vCornerRadii);
        float outerAlpha = 1.0 - smoothstep(-1.0, 1.0, d);
        float innerD = sdfRoundedBox(localPos, halfSize - vCornerRadii.x - vBorderWidth, vCornerRadii - vBorderWidth);
        float innerAlpha = 1.0 - smoothstep(-1.0, 1.0, innerD);
        float borderAlpha = outerAlpha * (1.0 - innerAlpha);
        vec3 color = mix(vColor.rgb, vBorderColor.rgb, borderAlpha);
        float alpha = outerAlpha * max(vColor.a, vBorderColor.a * borderAlpha);
        FragColor = vec4(color, alpha);
    }
    else if (drawType == 3)
    {
        FragColor = vColor;
    }
    else
    {
        FragColor = vColor;
    }
}
";

    #endregion

    #region 字段

    private readonly GLContext _gl;
    private uint _program;
    private uint _vao;
    private uint _vbo;
    private uint _ebo;
    private bool _initialized;
    private int _maxVertices;
    private int _maxIndices;

    #endregion

    #region 构造函数

    public GLUiRenderer(GLContext gl)
    {
        _gl = gl;
    }

    #endregion

    #region 公开方法

    public bool Initialize()
    {
        try
        {
            var vs = _gl.CompileShaderFromSource(GLContext.VertexShader, VertexShaderSource);
            if (vs == 0)
            {
                Console.WriteLine("[GLUiRenderer] 顶点着色器编译失败");
                return false;
            }

            var fs = _gl.CompileShaderFromSource(GLContext.FragmentShader, FragmentShaderSource);
            if (fs == 0)
            {
                Console.WriteLine("[GLUiRenderer] 片段着色器编译失败");
                _gl.DeleteShader(vs);
                return false;
            }

            _program = _gl.LinkProgramFromShaders(vs, fs);
            _gl.DeleteShader(vs);
            _gl.DeleteShader(fs);

            if (_program == 0)
            {
                Console.WriteLine("[GLUiRenderer] 着色器程序链接失败");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GLUiRenderer] 着色器编译/链接失败: {ex.Message}");
            return false;
        }

        try
        {
            _vao = _gl.GenVertexArray();
            _vbo = _gl.GenBuffer();
            _ebo = _gl.GenBuffer();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GLUiRenderer] VAO/VBO 创建失败（OpenGL 函数不可用）: {ex.Message}");
            return false;
        }

        _gl.BindVertexArray(_vao);

        _gl.BindBuffer(GLContext.ArrayBuffer, _vbo);
        _gl.BufferData(GLContext.ArrayBuffer, 0, null, GLContext.DynamicDraw);

        _gl.BindBuffer(GLContext.ElementArrayBuffer, _ebo);
        _gl.BufferData(GLContext.ElementArrayBuffer, 0, null, GLContext.DynamicDraw);

        int stride = sizeof(UIVertex);

        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(0, 3, GLContext.Float, false, stride, 0);

        _gl.EnableVertexAttribArray(1);
        _gl.VertexAttribPointer(1, 4, GLContext.Float, false, stride, 12);

        _gl.EnableVertexAttribArray(2);
        _gl.VertexAttribPointer(2, 2, GLContext.Float, false, stride, 28);

        _gl.EnableVertexAttribArray(3);
        _gl.VertexAttribPointer(3, 2, GLContext.Float, false, stride, 36);

        _gl.EnableVertexAttribArray(4);
        _gl.VertexAttribPointer(4, 4, GLContext.Float, false, stride, 44);

        _gl.EnableVertexAttribArray(5);
        _gl.VertexAttribPointer(5, 1, GLContext.Float, false, stride, 60);

        _gl.EnableVertexAttribArray(6);
        _gl.VertexAttribPointer(6, 1, GLContext.Float, false, stride, 64);

        _gl.EnableVertexAttribArray(7);
        _gl.VertexAttribPointer(7, 4, GLContext.Float, false, stride, 68);

        _gl.BindVertexArray(0);

        _initialized = true;
        Console.WriteLine("[GLUiRenderer] UI 渲染器初始化完成");
        return true;
    }

    public void Render(GpuWidgetRenderer widgetRenderer)
    {
        if (!_initialized)
        {
            return;
        }

        var (vertices, indices) = widgetRenderer.GetGpuData();
        if (vertices.Length == 0 || indices.Length == 0)
        {
            return;
        }

        _gl.Enable(GLContext.Blend);
        _gl.BlendFunc(GLContext.SrcAlpha, GLContext.OneMinusSrcAlpha);
        _gl.Disable(GLContext.DepthBufferBit);
        _gl.DepthMask(false);

        _gl.UseProgram(_program);

        var vertexDataSize = vertices.Length * sizeof(UIVertex);
        var indexDataSize = indices.Length * sizeof(uint);

        _gl.BindVertexArray(_vao);

        _gl.BindBuffer(GLContext.ArrayBuffer, _vbo);
        if (vertices.Length > _maxVertices)
        {
            fixed (UIVertex* pVerts = vertices)
            {
                _gl.BufferData(GLContext.ArrayBuffer, vertexDataSize, pVerts, GLContext.DynamicDraw);
            }
            _maxVertices = vertices.Length;
        }
        else
        {
            fixed (UIVertex* pVerts = vertices)
            {
                _gl.BufferSubData(GLContext.ArrayBuffer, 0, vertexDataSize, pVerts);
            }
        }

        _gl.BindBuffer(GLContext.ElementArrayBuffer, _ebo);
        if (indices.Length > _maxIndices)
        {
            fixed (uint* pIndices = indices)
            {
                _gl.BufferData(GLContext.ElementArrayBuffer, indexDataSize, pIndices, GLContext.DynamicDraw);
            }
            _maxIndices = indices.Length;
        }
        else
        {
            fixed (uint* pIndices = indices)
            {
                _gl.BufferSubData(GLContext.ElementArrayBuffer, 0, indexDataSize, pIndices);
            }
        }

        _gl.DrawElements(GLContext.Triangles, indices.Length, GLContext.UnsignedInt, 0);

        _gl.BindVertexArray(0);
        _gl.UseProgram(0);

        _gl.DepthMask(true);
        _gl.Disable(GLContext.Blend);
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_initialized)
        {
            if (_vao != 0) _gl.DeleteVertexArray(_vao);
            if (_vbo != 0) _gl.DeleteBuffer(_vbo);
            if (_ebo != 0) _gl.DeleteBuffer(_ebo);
            if (_program != 0) _gl.DeleteProgram(_program);
            _initialized = false;
        }
    }

    #endregion
}
