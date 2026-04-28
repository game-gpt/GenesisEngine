using Gnosis.Input.Device;
using Gnosis.Input.Simulate;

namespace Genesis.HAL.Adapters;

/// <summary>
/// IInputManager 的 InputSystem 适配器
/// </summary>
public sealed class InputSystemAdapter : IInputManager
{
    #region 字段

    private readonly InputSystem _inputSystem;

    #endregion

    #region 属性

    public bool IsInitialized => _inputSystem.Keyboard is not null && _inputSystem.Mouse is not null;

    public float MouseX => _inputSystem.Mouse?.Position.X ?? 0f;

    public float MouseY => _inputSystem.Mouse?.Position.Y ?? 0f;

    public float ScrollDelta => _inputSystem.Mouse?.ScrollDelta ?? 0f;

    #endregion

    #region 构造函数

    public InputSystemAdapter(InputSystem inputSystem)
    {
        _inputSystem = inputSystem;
    }

    #endregion

    #region IInputManager 实现

    public void Initialize()
    {
    }

    public void Update()
    {
        _inputSystem.Update();
    }

    public bool GetMouseButtonDown(int button)
    {
        return _inputSystem.Mouse?.GetButtonDown(button) ?? false;
    }

    public bool GetMouseButtonUp(int button)
    {
        return _inputSystem.Mouse?.GetButtonUp(button) ?? false;
    }

    public bool GetKeyDown(int keyCode)
    {
        return _inputSystem.Keyboard?.GetKeyDown(keyCode) ?? false;
    }

    #endregion
}
