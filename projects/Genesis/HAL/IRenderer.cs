namespace Genesis.HAL;

/// <summary>
/// 渲染器抽象接口
/// 内核通过此接口执行渲染操作，不直接依赖 Gnosis.Graphic.RHI
/// HAL 实现负责将调用委托给 Gnosis.Graphic.RHI
/// </summary>
public interface IRenderer : IDisposable
{
    /// <summary>
    /// 是否已初始化
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// 初始化渲染器
    /// </summary>
    /// <param name="backendType">后端类型名称（如 "OpenGL"、"Vulkan"）</param>
    void Initialize(string backendType);

    /// <summary>
    /// 自动检测最佳后端并初始化
    /// </summary>
    void InitializeAuto();

    /// <summary>
    /// 开始渲染帧
    /// </summary>
    void BeginFrame();

    /// <summary>
    /// 结束渲染帧
    /// </summary>
    void EndFrame();

    /// <summary>
    /// 清除渲染目标
    /// </summary>
    /// <param name="r">红色分量</param>
    /// <param name="g">绿色分量</param>
    /// <param name="b">蓝色分量</param>
    /// <param name="a">透明度分量</param>
    void Clear(float r, float g, float b, float a);
}
