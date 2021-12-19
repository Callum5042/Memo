using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Memo.WPF
{
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
            Tabs.Items.Add(new TabItem()
            {
                Header = "New Tab",
                IsSelected = true
            });


        }

        private void TabItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var menu = new Menu();

        }
    }
}
