using System.Runtime.InteropServices;
using Genesis.Terraria.Rendering;

namespace Genesis.Terraria.Platform;

public sealed class Win32Window : IDisposable
{
    #region Win32 P/Invoke

    [DllImport("user32.dll", EntryPoint = "CreateWindowExW")]
    private static extern nint CreateWindowExW(
        uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
        int x, int y, int nWidth, int nHeight,
        nint hWndParent, nint hMenu, nint hInstance, nint lpParam);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(nint hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool PeekMessageW(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern nint DispatchMessageW(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern void PostQuitMessage(int nExitCode);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(nint hWnd);

    [DllImport("user32.dll")]
    private static extern bool UpdateWindow(nint hWnd);

    [DllImport("user32.dll")]
    private static extern nint DefWindowProcW(nint hWnd, uint Msg, nint wParam, nint lParam);

    [DllImport("user32.dll", EntryPoint = "RegisterClassW")]
    private static extern ushort RegisterClassW(ref WNDCLASSW lpWndClass);

    [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW")]
    private static extern nint GetModuleHandleW(nint lpModuleName);

    [DllImport("user32.dll")]
    private static extern nint BeginPaint(nint hWnd, out PAINTSTRUCT lpPaint);

    [DllImport("user32.dll")]
    private static extern nint EndPaint(nint hWnd, ref PAINTSTRUCT lpPaint);

    [DllImport("gdi32.dll")]
    private static extern nint CreateCompatibleDC(nint hdc);

    [DllImport("gdi32.dll")]
    private static extern nint CreateDIBSection(nint hdc, ref BITMAPINFO pbmi, uint iUsage, out nint ppvBits, nint hSection, uint dwOffset);

    [DllImport("gdi32.dll")]
    private static extern nint SelectObject(nint hdc, nint h);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(nint hdc, int x, int y, int cx, int cy, nint hdcSrc, int x1, int y1, uint rop);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(nint hObject);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(nint hdc);

    [DllImport("user32.dll")]
    private static extern bool InvalidateRect(nint hWnd, nint lpRect, bool bErase);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASSW
    {
        public uint style;
        public nint lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public nint hInstance;
        public nint hIcon;
        public nint hCursor;
        public nint hbrBackground;
        public string? lpszMenuName;
        public string? lpszClassName;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public nint hwnd;
        public uint message;
        public nint wParam;
        public nint lParam;
        public uint time;
        public int pt_x;
        public int pt_y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PAINTSTRUCT
    {
        public nint hdc;
        [MarshalAs(UnmanagedType.Bool)] public bool fErase;
        public int rcPaint_left;
        public int rcPaint_top;
        public int rcPaint_right;
        public int rcPaint_bottom;
        [MarshalAs(UnmanagedType.Bool)] public bool fRestore;
        [MarshalAs(UnmanagedType.Bool)] public bool fIncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] rgbReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFO
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    private const int WM_DESTROY = 0x0002;
    private const int WM_PAINT = 0x000F;
    private const int WM_KEYDOWN = 0x0100;
    private const uint PM_REMOVE = 0x0001;
    private const int SW_SHOWDEFAULT = 10;
    private const uint SRCCOPY = 0x00CC0020;
    private const int DIB_RGB_COLORS = 0;

    #endregion

    #region 私有字段

    private nint _hwnd;
    private nint _hInstance;
    private nint _memDC;
    private nint _dibSection;
    private nint _dibBits;
    private nint _oldBitmap;

    private readonly int _width;
    private readonly int _height;
    private bool _isRunning;
    private bool _isDisposed;

    private delegate nint WndProcDelegate(nint hWnd, uint msg, nint wParam, nint lParam);
    private readonly WndProcDelegate _wndProc;

    private readonly HashSet<int> _keysDown = [];
    private readonly HashSet<int> _keysJustPressed = [];

    #endregion

    #region 公开属性

    public bool IsRunning => _isRunning;
    public nint WindowHandle => _hwnd;

    #endregion

    #region 构造函数

    public Win32Window(int width, int height, string title = "Genesis Engine")
    {
        _width = width;
        _height = height;
        _wndProc = WndProc;

        _hInstance = GetModuleHandleW(nint.Zero);

        var wc = new WNDCLASSW
        {
            style = 0,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProc),
            hInstance = _hInstance,
            hCursor = nint.Zero,
            hbrBackground = nint.Zero,
            lpszClassName = "GenesisWindowClass"
        };

        RegisterClassW(ref wc);

        var windowWidth = width + 16;
        var windowHeight = height + 39;

        _hwnd = CreateWindowExW(
            0, "GenesisWindowClass", title,
            0x10CF0000,
            int.MaxValue, int.MaxValue, windowWidth, windowHeight,
            nint.Zero, nint.Zero, _hInstance, nint.Zero);

        CreateDIB();

        ShowWindow(_hwnd, SW_SHOWDEFAULT);
        UpdateWindow(_hwnd);

        _isRunning = true;
    }

    #endregion

    #region DIB 创建

    private void CreateDIB()
    {
        _memDC = CreateCompatibleDC(nint.Zero);

        var bmi = new BITMAPINFO
        {
            biSize = (uint)Marshal.SizeOf<BITMAPINFO>(),
            biWidth = _width,
            biHeight = -_height,
            biPlanes = 1,
            biBitCount = 32,
            biCompression = 0
        };

        _dibSection = CreateDIBSection(_memDC, ref bmi, DIB_RGB_COLORS, out _dibBits, nint.Zero, 0);
        _oldBitmap = SelectObject(_memDC, _dibSection);
    }

    #endregion

    #region 消息循环

    public bool ProcessMessages()
    {
        _keysJustPressed.Clear();

        while (PeekMessageW(out var msg, nint.Zero, 0, 0, PM_REMOVE))
        {
            if (msg.message == WM_KEYDOWN)
            {
                var vkCode = (int)msg.wParam;
                if (!_keysDown.Contains(vkCode))
                {
                    _keysJustPressed.Add(vkCode);
                }

                _keysDown.Add(vkCode);
            }
            else if (msg.message == 0x0101)
            {
                _keysDown.Remove((int)msg.wParam);
            }

            TranslateMessage(ref msg);
            DispatchMessageW(ref msg);
        }

        return _isRunning;
    }

    private nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        if (msg == WM_DESTROY)
        {
            _isRunning = false;
            PostQuitMessage(0);
            return nint.Zero;
        }

        if (msg == WM_PAINT)
        {
            var hdc = BeginPaint(hWnd, out var ps);
            BitBlt(hdc, 0, 0, _width, _height, _memDC, 0, 0, SRCCOPY);
            EndPaint(hWnd, ref ps);
            return nint.Zero;
        }

        return DefWindowProcW(hWnd, msg, wParam, lParam);
    }

    #endregion

    #region 帧缓冲上传

    public unsafe void PresentFrame(byte[] rgbaData)
    {
        if (_dibBits == nint.Zero || rgbaData.Length < _width * _height * 4)
        {
            return;
        }

        var destPtr = (byte*)_dibBits;
        var srcIdx = 0;

        for (var i = 0; i < _width * _height; i++)
        {
            destPtr[0] = rgbaData[srcIdx + 2];
            destPtr[1] = rgbaData[srcIdx + 1];
            destPtr[2] = rgbaData[srcIdx];
            destPtr[3] = 255;
            destPtr += 4;
            srcIdx += 4;
        }

        InvalidateRect(_hwnd, nint.Zero, false);
    }

    #endregion

    #region 输入

    public bool IsKeyDown(int vkCode) => _keysDown.Contains(vkCode);

    public bool IsKeyJustPressed(int vkCode)
    {
        if (_keysJustPressed.Contains(vkCode))
        {
            _keysJustPressed.Remove(vkCode);
            return true;
        }

        return false;
    }

    public static class VirtualKey
    {
        public const int Left = 0x25;
        public const int Up = 0x26;
        public const int Right = 0x27;
        public const int Down = 0x28;
        public const int A = 0x41;
        public const int D = 0x44;
        public const int E = 0x45;
        public const int I = 0x49;
        public const int J = 0x4A;
        public const int K = 0x4B;
        public const int Q = 0x51;
        public const int S = 0x53;
        public const int W = 0x57;
        public const int Space = 0x20;
        public const int Escape = 0x1B;
        public const int D1 = 0x31;
        public const int D2 = 0x32;
        public const int D3 = 0x33;
        public const int D4 = 0x34;
        public const int D5 = 0x35;
        public const int D6 = 0x36;
        public const int D7 = 0x37;
        public const int D8 = 0x38;
        public const int D9 = 0x39;
        public const int D0 = 0x30;
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        if (_hwnd != nint.Zero)
        {
            DestroyWindow(_hwnd);
        }

        if (_dibSection != nint.Zero)
        {
            SelectObject(_memDC, _oldBitmap);
            DeleteObject(_dibSection);
        }

        if (_memDC != nint.Zero)
        {
            DeleteDC(_memDC);
        }

        _isDisposed = true;
    }

    #endregion
}
