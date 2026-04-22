namespace Genesis.Terraria.GUI;

public sealed class InputState
{
    private readonly HashSet<int> _keysDown = [];
    private readonly HashSet<int> _keysPressed = [];

    public void SetKey(int keyCode, bool pressed)
    {
        if (pressed)
        {
            if (!_keysDown.Contains(keyCode))
            {
                _keysPressed.Add(keyCode);
            }

            _keysDown.Add(keyCode);
        }
        else
        {
            _keysDown.Remove(keyCode);
        }
    }

    public bool IsKeyDown(int keyCode) => _keysDown.Contains(keyCode);

    public bool IsKeyJustPressed(int keyCode)
    {
        if (_keysPressed.Contains(keyCode))
        {
            _keysPressed.Remove(keyCode);
            return true;
        }

        return false;
    }

    public void ClearFrameState()
    {
        _keysPressed.Clear();
    }
}
