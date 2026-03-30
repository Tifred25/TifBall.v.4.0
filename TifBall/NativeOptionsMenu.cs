using System;
using System.Runtime.InteropServices;

namespace TifBall;

internal sealed class NativeOptionsMenu : IDisposable
{
    private const uint WmCommand = 0x0111;
    private const int GwlWndProc = -4;
    private const uint MfString = 0x00000000;
    private const uint MfPopup = 0x00000010;
    private const uint MfByCommand = 0x00000000;
    private const uint MfChecked = 0x00000008;
    private const uint MfUnchecked = 0x00000000;
    private const uint MfEnabled = 0x00000000;
    private const uint MfGrayed = 0x00000001;

    private const uint IdPause = 1001;
    private const uint IdMusic = 1002;
    private const uint IdSounds = 1003;
    private const uint IdBackgroundImages = 1004;
    private const uint IdAbout = 1005;

    private readonly Action _togglePause;
    private readonly Action _toggleMusic;
    private readonly Action _toggleSounds;
    private readonly Action _toggleBackgroundImages;
    private readonly Action _showAbout;
    private readonly WndProcDelegate _wndProcDelegate;

    private IntPtr _windowHandle;
    private IntPtr _mainMenu;
    private IntPtr _optionsMenu;
    private IntPtr _helpMenu;
    private IntPtr _previousWndProc;

    public NativeOptionsMenu(
        IntPtr windowHandle,
        Action togglePause,
        Action toggleMusic,
        Action toggleSounds,
        Action toggleBackgroundImages,
        Action showAbout)
    {
        _windowHandle = windowHandle;
        _togglePause = togglePause;
        _toggleMusic = toggleMusic;
        _toggleSounds = toggleSounds;
        _toggleBackgroundImages = toggleBackgroundImages;
        _showAbout = showAbout;
        _wndProcDelegate = WindowProc;

        CreateAndAttachMenu();
    }

    public void UpdateState(bool isPaused, bool musicEnabled, bool soundsEnabled, bool backgroundImagesEnabled, bool pauseEnabled, bool visible)
    {
        if (_optionsMenu == IntPtr.Zero)
        {
            return;
        }

        CheckMenuItem(_optionsMenu, IdPause, MfByCommand | (isPaused ? MfChecked : MfUnchecked));
        CheckMenuItem(_optionsMenu, IdMusic, MfByCommand | (musicEnabled ? MfChecked : MfUnchecked));
        CheckMenuItem(_optionsMenu, IdSounds, MfByCommand | (soundsEnabled ? MfChecked : MfUnchecked));
        CheckMenuItem(_optionsMenu, IdBackgroundImages, MfByCommand | (backgroundImagesEnabled ? MfChecked : MfUnchecked));
        EnableMenuItem(_optionsMenu, IdPause, MfByCommand | (pauseEnabled ? MfEnabled : MfGrayed));

        SetMenu(_windowHandle, visible ? _mainMenu : IntPtr.Zero);
        DrawMenuBar(_windowHandle);
    }

    public void Dispose()
    {
        if (_windowHandle != IntPtr.Zero && _previousWndProc != IntPtr.Zero)
        {
            SetWindowLongPtr(_windowHandle, GwlWndProc, _previousWndProc);
            _previousWndProc = IntPtr.Zero;
        }

        if (_windowHandle != IntPtr.Zero)
        {
            SetMenu(_windowHandle, IntPtr.Zero);
            DrawMenuBar(_windowHandle);
        }

        if (_mainMenu != IntPtr.Zero)
        {
            DestroyMenu(_mainMenu);
            _mainMenu = IntPtr.Zero;
            _optionsMenu = IntPtr.Zero;
            _helpMenu = IntPtr.Zero;
        }

        _windowHandle = IntPtr.Zero;
    }

    private void CreateAndAttachMenu()
    {
        _mainMenu = CreateMenu();
        _optionsMenu = CreatePopupMenu();
        _helpMenu = CreatePopupMenu();

        AppendMenu(_optionsMenu, MfString, IdPause, "Pause");
        AppendMenu(_optionsMenu, MfString, IdMusic, "Musique");
        AppendMenu(_optionsMenu, MfString, IdSounds, "Sons");
        AppendMenu(_optionsMenu, MfString, IdBackgroundImages, "Images de fond");
        AppendMenu(_helpMenu, MfString, IdAbout, "A propos");
        AppendMenu(_mainMenu, MfPopup, (nuint)_optionsMenu, "Options");
        AppendMenu(_mainMenu, MfPopup, (nuint)_helpMenu, "?");

        SetMenu(_windowHandle, _mainMenu);
        DrawMenuBar(_windowHandle);
        _previousWndProc = SetWindowLongPtr(_windowHandle, GwlWndProc, Marshal.GetFunctionPointerForDelegate(_wndProcDelegate));
    }

    private IntPtr WindowProc(IntPtr hwnd, uint message, nuint wParam, nint lParam)
    {
        if (message == WmCommand)
        {
            switch ((uint)(wParam & 0xFFFF))
            {
                case IdPause:
                    _togglePause();
                    return IntPtr.Zero;
                case IdMusic:
                    _toggleMusic();
                    return IntPtr.Zero;
                case IdSounds:
                    _toggleSounds();
                    return IntPtr.Zero;
                case IdBackgroundImages:
                    _toggleBackgroundImages();
                    return IntPtr.Zero;
                case IdAbout:
                    _showAbout();
                    return IntPtr.Zero;
            }
        }

        return CallWindowProc(_previousWndProc, hwnd, message, wParam, lParam);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "AppendMenuW", SetLastError = true)]
    private static extern bool AppendMenu(IntPtr hMenu, uint uFlags, nuint uIDNewItem, string lpNewItem);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool CheckMenuItem(IntPtr hMenu, uint uIDCheckItem, uint uCheck);

    [DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, nuint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CreateMenu();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CreatePopupMenu();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyMenu(IntPtr hMenu);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DrawMenuBar(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetMenu(IntPtr hWnd, IntPtr hMenu);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    private delegate IntPtr WndProcDelegate(IntPtr hwnd, uint message, nuint wParam, nint lParam);
}
