using System;
using System.Text;
using System.Runtime.InteropServices;


namespace EDMCOverlay
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }


    public class WindowUtils
    {
        public const Int32 WS_EX_LAYERED = 0x00080000;
        public const Int32 WS_EX_TRANSPARENT = 0x00000020;
        public const Int32 WS_EX_NOACTIVATE = 0x08000000;

        public const int GWL_EXSTYLE = -20;

        public const int DWMWA_TRANSITIONS_FORCEDISABLED = 3;

        [DllImport("dwmapi.dll")]
        public static extern UInt32 DwmSetWindowAttribute(IntPtr hWnd, UInt32 attr, ref int value, UInt32 attrLen);

        [DllImport("user32.dll")]
        public static extern Int32 SetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern Int32 GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);


        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
    }

}
