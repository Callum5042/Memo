using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

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
        private const int APPWM_ICONNOTIFY = 0x8000 + 1;
        private NotifyIconData _notifyIconData = new NotifyIconData();

        public TrayIcon(Window window)
        {
            var wih = new WindowInteropHelper(window);

            _notifyIconData.cbSize = (uint)Marshal.SizeOf(_notifyIconData);
            _notifyIconData.hWnd = wih.Handle;
            _notifyIconData.uFlags = NotifyIconFlags.NIF_GUID | NotifyIconFlags.NIF_MESSAGE;
            _notifyIconData.uCallbackMessage = APPWM_ICONNOTIFY;

            // Hook WndProc
            HwndSource source = (HwndSource)PresentationSource.FromVisual(window);
            source.AddHook(WndProc);
        }

        public string ToolTip 
        {
            get => _notifyIconData.szTip;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _notifyIconData.uFlags &= ~NotifyIconFlags.NIF_TIP;
                }
                else
                {
                    _notifyIconData.szTip = value;
                    if (!_notifyIconData.uFlags.HasFlag(NotifyIconFlags.NIF_TIP))
                    {
                        _notifyIconData.uFlags |= NotifyIconFlags.NIF_TIP;
                    }
                }
            }
        }

        public Action? LeftMouseButtonUp { get; set; }

        public Action? RightMouseButtonUp { get; set; }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == APPWM_ICONNOTIFY)
            {
                const int WM_LBUTTONUP = 0x0202;
                const int WM_RBUTTONUP = 0x0205;

                switch ((int)lParam)
                {
                    case WM_LBUTTONUP:
                        LeftMouseButtonUp?.Invoke();
                        break;

                    case WM_RBUTTONUP:
                        RightMouseButtonUp?.Invoke();
                        break;
                }
            }

            return IntPtr.Zero;
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
            GC.SuppressFinalize(this);
            Shell_NotifyIcon(TrayMessage.NIM_DELETE, _notifyIconData);
        }

        public void UpdateIcon()
        {
            Shell_NotifyIcon(TrayMessage.NIM_MODIFY, _notifyIconData);
        }

        private Icon? _icon;

        public Icon Icon
        {
            set
            {
                if (value == null)
                {
                    // Remove NIF_ICON flag if null
                    _notifyIconData.uFlags &= ~NotifyIconFlags.NIF_ICON;
                    _icon = null;
                    return;
                }
                else
                {
                    // Set value to hidden field so GC doesn't clean up the resource (since we only want the pointer)
                    _icon = value;

                    // Add flag if new
                    _notifyIconData.hIcon = _icon.Handle;
                    if (!_notifyIconData.uFlags.HasFlag(NotifyIconFlags.NIF_ICON))
                    {
                        _notifyIconData.uFlags |= NotifyIconFlags.NIF_ICON;
                    }
                }
            }
        }
    }
}
