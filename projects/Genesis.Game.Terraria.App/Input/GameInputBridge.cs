using System.Windows.Input;
using Gnosis.Input.Simulate;
using GnosisKeyboard = Gnosis.Input.Device.Keyboard;
using GnosisMouse = Gnosis.Input.Device.Mouse;
using GnosisMouseButton = Gnosis.Input.Device.MouseButton;
using WpfKey = System.Windows.Input.Key;
using WpfMouseButton = System.Windows.Input.MouseButton;

namespace Genesis.Game.Terraria.Input;

public sealed class GameInputBridge
{
    private readonly InputSystem _inputSystem;
    private readonly GnosisKeyboard _keyboard;
    private readonly GnosisMouse _mouse;

    public GameInputBridge(InputSystem inputSystem)
    {
        _inputSystem = inputSystem;
        _keyboard = (GnosisKeyboard)inputSystem.Keyboard!;
        _mouse = (GnosisMouse)inputSystem.Mouse!;
    }

    public void Update()
    {
        _inputSystem.Update();
    }

    public void OnKeyDown(object sender, KeyEventArgs e)
    {
        var keyCode = ConvertKey(e.Key);
        if (keyCode >= 0)
        {
            _keyboard.SetKeyState(keyCode, true);
        }
    }

    public void OnKeyUp(object sender, KeyEventArgs e)
    {
        var keyCode = ConvertKey(e.Key);
        if (keyCode >= 0)
        {
            _keyboard.SetKeyState(keyCode, false);
        }
    }

    public void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var button = ConvertMouseButton(e.ChangedButton);
        if (button >= 0)
        {
            _mouse.SetButtonState(button, true);
        }
    }

    public void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        var button = ConvertMouseButton(e.ChangedButton);
        if (button >= 0)
        {
            _mouse.SetButtonState(button, false);
        }
    }

    public void OnMouseMove(object sender, MouseEventArgs e)
    {
        var pos = e.GetPosition(null);
        _mouse.SetPosition((float)pos.X, (float)pos.Y);
    }

    private static int ConvertKey(WpfKey key)
    {
        return key switch
        {
            WpfKey.A => KeyCode.A,
            WpfKey.D => KeyCode.D,
            WpfKey.S => KeyCode.S,
            WpfKey.W => KeyCode.W,
            WpfKey.Left => KeyCode.Left,
            WpfKey.Right => KeyCode.Right,
            WpfKey.Up => KeyCode.Up,
            WpfKey.Down => KeyCode.Down,
            WpfKey.Space => KeyCode.Space,
            WpfKey.Escape => KeyCode.Escape,
            WpfKey.D0 => 48,
            WpfKey.D1 => 49,
            WpfKey.D2 => 50,
            WpfKey.D3 => 51,
            WpfKey.D4 => 52,
            WpfKey.D5 => 53,
            WpfKey.D6 => 54,
            WpfKey.D7 => 55,
            WpfKey.D8 => 56,
            WpfKey.D9 => 57,
            _ => -1
        };
    }

    private static int ConvertMouseButton(WpfMouseButton button)
    {
        return button switch
        {
            WpfMouseButton.Left => GnosisMouseButton.Left,
            WpfMouseButton.Right => GnosisMouseButton.Right,
            WpfMouseButton.Middle => GnosisMouseButton.Middle,
            _ => -1
        };
    }
}
