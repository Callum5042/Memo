using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct NotifyIconData
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uID;
        public NotifyIconFlags uFlags;
        public uint uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public uint dwState;
        public uint dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public UnionName unionName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public uint dwInfoFlags;
        public Guid guidItem;
        public IntPtr hBalloonIcon;
    }

    public class TrayIcon : IDisposable
    {
        private NotifyIconData _notifyIconData = new NotifyIconData();

        public TrayIcon(IntPtr hWnd)
        {
            _notifyIconData.cbSize = (uint)Marshal.SizeOf(_notifyIconData);
            _notifyIconData.hWnd = hWnd;
            _notifyIconData.szTip = "Test Application";
            _notifyIconData.uFlags = NotifyIconFlags.NIF_ICON | NotifyIconFlags.NIF_TIP | NotifyIconFlags.NIF_GUID | NotifyIconFlags.NIF_MESSAGE;

            SetIcon(@"memo.ico");
        }

        [DllImport("Shell32.dll", EntryPoint = "Shell_NotifyIconW")]
        private static extern bool Shell_NotifyIcon(TrayMessage dwMessage, NotifyIconData notifyIconData);

        private enum TrayMessage : uint
        {
            NIM_ADD = 0x00000000,
            NIM_MODIFY = 0x00000001,
            NIM_DELETE = 0x00000002,
            NIM_SETFOCUS = 0x00000003,
            NIM_SETVERSION = 0x00000004
        }

        public void AddIcon()
        {
            var result = Shell_NotifyIcon(TrayMessage.NIM_ADD, _notifyIconData);
            if (!result)
            {
                return;
            }
        }

        public void Dispose()
        {
            Shell_NotifyIcon(TrayMessage.NIM_DELETE, _notifyIconData);
        }

        public void UpdateIcon()
        {
            Shell_NotifyIcon(TrayMessage.NIM_MODIFY, _notifyIconData);
        }

        void SetIcon(Stream stream)
        {
            var bitmap = (Bitmap)Image.FromStream(stream);
            _notifyIconData.hIcon = bitmap.GetHicon();
        }

        void SetIcon(string path)
        {
            var bitmap = (Bitmap)Image.FromFile(path);
            _notifyIconData.hIcon = bitmap.GetHicon();
        }
    }
}
