using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Memo.WPF
{
    public static class WindowsServices
    {
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public static void SetWindowExTransparent(IntPtr hwnd)
        {
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_NewTab_Click(object sender, RoutedEventArgs e)
        {
            // Add new tab and auto switch to that tab
            //Tabs.Items.Add(new TabItem()
            //{
            //    Header = "New Tab",
            //    IsSelected = true
            //});
        }

        private void MenuItem_PopOut_Click(object sender, RoutedEventArgs e)
        {
            var window = new Window
            {
                ResizeMode = ResizeMode.CanResizeWithGrip,
                WindowStyle = WindowStyle.None,
                Topmost = true,
                Width = 200,
                Height = 100,
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Colors.LightGray),
                AllowsTransparency = true,
                ShowInTaskbar = false
            };

            window.BorderThickness = new Thickness(1.0);
            //window.Background.Opacity = 0.5;

            // Enable mouse events to pass through the window
            //window.SourceInitialized += (sender, e) =>
            //{
            //    var hwnd = new WindowInteropHelper(window).Handle;
            //    WindowsServices.SetWindowExTransparent(hwnd);
            //};
            
            // Allow window to be dragged anywhere
            window.MouseDown += (sender, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    window.DragMove();
                }
            };

            // Add text
            var text = new TextBlock();
            text.Text = "Hello, World!";
            text.Padding = new Thickness(5.0, 0.0, 5.0, 0.0);

            window.Content = text;

            window.Show();
        }
    }
}
