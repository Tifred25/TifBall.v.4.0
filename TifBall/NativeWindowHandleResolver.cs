using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TifBall;

internal static class NativeWindowHandleResolver
{
    private const uint GwOwner = 4;

    public static IntPtr TryFindCurrentProcessMainWindow(string titlePrefix)
    {
        IntPtr fallbackHandle = IntPtr.Zero;
        uint processId = (uint)Environment.ProcessId;

        EnumWindows((windowHandle, _) =>
        {
            if (!IsWindowVisible(windowHandle) || GetWindow(windowHandle, GwOwner) != IntPtr.Zero)
            {
                return true;
            }

            GetWindowThreadProcessId(windowHandle, out uint ownerProcessId);
            if (ownerProcessId != processId)
            {
                return true;
            }

            string title = GetWindowTitle(windowHandle);
            if (!string.IsNullOrEmpty(title))
            {
                if (title.StartsWith(titlePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    fallbackHandle = windowHandle;
                    return false;
                }

                if (fallbackHandle == IntPtr.Zero)
                {
                    fallbackHandle = windowHandle;
                }
            }

            return true;
        }, IntPtr.Zero);

        return fallbackHandle;
    }

    private static string GetWindowTitle(IntPtr windowHandle)
    {
        int length = GetWindowTextLength(windowHandle);
        if (length == 0)
        {
            return string.Empty;
        }

        StringBuilder buffer = new(length + 1);
        GetWindowText(windowHandle, buffer, buffer.Capacity);
        return buffer.ToString();
    }

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
}
