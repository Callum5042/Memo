using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Memo.WPF
{
    [StructLayout(LayoutKind.Explicit)]
    public struct UnionName
    {
        [FieldOffset(0)]
        uint uTimeout;

        [FieldOffset(0)]
        uint uVersion;
    }

    [Flags]
    public enum NotifyIconFlags : uint
    {
        NIF_MESSAGE = 0x00000001,
        NIF_ICON = 0x00000002,
        NIF_TIP = 0x00000004,
        NIF_STATE = 0x00000008,
        NIF_INFO = 0x00000010,
        NIF_GUID = 0x00000020,
        NIF_REALTIME = 0x00000040,
        NIF_SHOWTIP = 0x00000080
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NotifyIconData
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uID;
        public NotifyIconFlags uFlags;
        public uint uCallbackMessage;
        public IntPtr hIcon;
        public string szTip;
        public uint dwState;
        public uint dwStateMask;
        public string szInfo;
        public UnionName unionName;
        public string szInfoTitle;
        public uint dwInfoFlags;
        public Guid guidItem;
        public IntPtr hBalloonIcon;
    }

    public class TrayIcon
    {
        private readonly IntPtr _hWnd;

        public TrayIcon(IntPtr hWnd)
        {
            _hWnd = hWnd;
        }

        [DllImport("Shell32.dll")]
        private static extern bool Shell_NotifyIcon(TrayMessage dwMessage, NotifyIconData notifyIconData);

        private enum TrayMessage : uint
        {
            NIM_ADD = 0x00000000,
            NIM_MODIFY = 0x00000001,
            NIM_DELETE = 0x00000002,
            NIM_SETFOCUS = 0x00000003,
            NIM_SETVERSION = 0x00000004
        }

        public void AddIcon(NotifyIconData notifyIconData)
        {
            var result = Shell_NotifyIcon(TrayMessage.NIM_ADD, notifyIconData);
            if (!result)
            {
                throw new InvalidOperationException("Shell_NotifyIcon failed");
            }
        }
    }
}
