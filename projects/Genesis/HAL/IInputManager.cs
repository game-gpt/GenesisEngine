namespace Genesis.HAL;

/// <summary>
/// 输入管理器抽象接口
/// 内核通过此接口获取输入状态，不直接依赖 Gnosis.Input
/// HAL 实现负责将调用委托给 Gnosis.Input
/// </summary>
public interface IInputManager
{
    /// <summary>
    /// 是否已初始化
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// 初始化输入管理器
    /// </summary>
    void Initialize();

    /// <summary>
    /// 帧更新
    /// </summary>
    void Update();

    /// <summary>
    /// 鼠标 X 坐标
    /// </summary>
    float MouseX { get; }

    /// <summary>
    /// 鼠标 Y 坐标
    /// </summary>
    float MouseY { get; }

    /// <summary>
    /// 鼠标滚轮增量
    /// </summary>
    float ScrollDelta { get; }

    /// <summary>
    /// 鼠标按钮是否按下
    /// </summary>
    /// <param name="button">按钮索引</param>
    /// <returns>是否按下</returns>
    bool GetMouseButtonDown(int button);

    /// <summary>
    /// 鼠标按钮是否松开
    /// </summary>
    /// <param name="button">按钮索引</param>
    /// <returns>是否松开</returns>
    bool GetMouseButtonUp(int button);

    /// <summary>
    /// 键盘按键是否按下
    /// </summary>
    /// <param name="keyCode">按键码</param>
    /// <returns>是否按下</returns>
    bool GetKeyDown(int keyCode);
}
