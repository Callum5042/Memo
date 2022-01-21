using Memo.WPF.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Add system tray icon
            _trayIcon = new TrayIcon(this)
            {
                ToolTip = "Testing",
                Icon = Properties.Resources.SystemTrayIcon
            };

            _trayIcon.AddIcon();

            // System tray icon events
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

            // Settings
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var memoPath = Path.Combine(appData, "Memo");
            if (!Directory.Exists(memoPath))
            {
                Directory.CreateDirectory(memoPath);
            }

            // Check if file exists
            var memoSettingsPath = Path.Combine(memoPath, "settings.json");
            if (!File.Exists(memoSettingsPath))
            {
                // Add default 'New Tab'
                var tab = new Tab() { Title = "New Tab" };
                _tabs.Add(tab);
                Tabs.SelectedItem = tab;
            }
            else
            {
                using var file = new FileStream(memoSettingsPath, FileMode.Open);
                using var reader = new StreamReader(file);
                var json = await reader.ReadToEndAsync();

                if (!string.IsNullOrEmpty(json))
                {
                    var settings = JsonSerializer.Deserialize<Settings>(json);
                    if (settings != null)
                    {
                        foreach (var tabSetting in settings.TabSettings)
                        {
                            if (!string.IsNullOrEmpty(tabSetting.Path))
                            {
                                using var tabSettingFile = File.OpenRead(tabSetting.Path);
                                using var tabSettingReader = new StreamReader(tabSettingFile);

                                var tab = new Tab()
                                {
                                    Text = await tabSettingReader.ReadToEndAsync(),
                                    Title = tabSetting.Name
                                };

                                _tabs.Add(tab);
                                Tabs.SelectedItem = tab;
                            }
                        }

                    }
                }
            }

            // Load tab thing
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

                // Save to settings.json
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var memoPath = Path.Combine(appData, "Memo");

                var memoSettingsPath = Path.Combine(memoPath, "settings.json");
                using var settingsFile = new FileStream(memoSettingsPath, FileMode.Open);
                using var writer = new StreamWriter(settingsFile);

                var settings = new Settings()
                {
                    TabSettings = new List<TabSettings>()
                    {
                        new TabSettings()
                        {
                            Name = dialog.SafeFileName,
                            Path = dialog.FileName
                        }
                    }
                };

                var json = JsonSerializer.Serialize(settings);
                await writer.WriteAsync(json);
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
