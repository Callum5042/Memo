using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace Memo.WPF.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<Tab> _tabs = new ObservableCollection<Tab>();
        private bool _canClose = false;
        private TrayIcon? _trayIcon = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Add system tray icon
            _trayIcon = new TrayIcon(this);
            _trayIcon.ToolTip = "Testing";
            _trayIcon.SetIcon(@"app.ico");

            _trayIcon.AddIcon();

            _trayIcon.LeftMouseButtonUp += () => RestoreWindow();

            _trayIcon.RightMouseButtonUp += () =>
            {
                if (FindResource("systemTrayContextMenu") is ContextMenu contextMenu)
                {
                    var wih = new WindowInteropHelper(this);
                    SetForegroundWindow(wih.Handle);
                    contextMenu.IsOpen = true;
                }
            };

            // Load tab thing
            var tab = new Tab() { Title = "New Tab" };
            _tabs.Add(tab);
            Tabs.SelectedItem = tab;

            DataContext = _tabs;
        }

        private void RestoreWindow()
        {
            ShowInTaskbar = true;
            Visibility = Visibility.Visible;
        }

        private void MenuItem_NewTab_Click(object sender, RoutedEventArgs e)
        {
            var tab = new Tab() { Title = "New Tab" };
            _tabs.Add(tab);
            Tabs.SelectedItem = tab;
        }

        private void MenuItem_PopOut_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem?.DataContext != null)
            {
                if (menuItem.DataContext is Tab tab)
                {
                    var window = new PopoutWindow();
                    window.TextBlock.Text = tab.Text;

                    window.Show();
                }
            }
        }

        private void MenuItem_RenameTab_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Rename tab");
        }

        private void MenuItem_CloseTab_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem?.DataContext != null)
            {
                if (menuItem.DataContext is Tab tab)
                {
                    _tabs.Remove(tab);
                }
            }
        }

        private async void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                DefaultExt = ".txt",
                Filter = "Text documents (.txt)|*.txt"
            };

            var result = dialog.ShowDialog();
            if (result == true)
            {
                // Read file
                using var file = File.OpenRead(dialog.FileName);
                using var reader = new StreamReader(file);
                var text = await reader.ReadToEndAsync();

                // Open new tab
                var tab = new Tab()
                {
                    Title = dialog.SafeFileName,
                    Text = text
                };

                _tabs.Add(tab);
                Tabs.SelectedItem = tab;
            }
        }

        private async void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {
            if (Tabs.SelectedItem is Tab activeTab)
            {
                // Open dialog
                var dialog = new SaveFileDialog()
                {
                    DefaultExt = ".txt",
                    Filter = "Text documents (.txt)|*.txt"
                };

                var result = dialog.ShowDialog();
                if (result == true)
                {
                    // Save file
                    using var file = File.OpenWrite(dialog.FileName);
                    using var writer = new StreamWriter(file);
                    await writer.WriteAsync(activeTab.Text);
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Only close if we have selected exit from the menu, otherwise minimise to the taskbar
            if (_canClose)
            {
                _trayIcon?.Dispose();
                return;
            }

            e.Cancel = true;
            ShowInTaskbar = false;
            Visibility = Visibility.Hidden;
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            _canClose = true;
            Close();
        }

        private void SystemTray_Click_Exit(object sender, RoutedEventArgs e)
        {
            _canClose = true;
            Close();
        }

        private void SystemTray_Click_Restore(object sender, RoutedEventArgs e)
        {
            RestoreWindow();
        }
    }
}
