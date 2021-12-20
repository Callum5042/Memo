using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

namespace Memo.WPF.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<Tab> _tabs = new ObservableCollection<Tab>();

        public MainWindow()
        {
            InitializeComponent();

            // Load tab thing
            var tab = new Tab() { Title = "New Tab" };
            _tabs.Add(tab);
            Tabs.SelectedItem = tab;

            DataContext = _tabs;
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
                    text.Text = tab.Text;
                    text.Padding = new Thickness(5.0, 0.0, 5.0, 0.0);

                    window.Content = text;

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
    }
}
