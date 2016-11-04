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

namespace PTZControlClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new PTZControlViewModel();
        }
        
        private void open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btOpenSerial.IsEnabled = false;
                PTZControlViewModel model = this.DataContext as PTZControlViewModel;
                if (model != null)
                {
                    model.Open();
                    new PTZControlWindow(model).Show();
                    this.Close();
                }
            }
            catch(Exception ex)
            {
                (this.DataContext as PTZControlViewModel)?.Disconnect();
                MessageBox.Show(ex.Message);
            }
            btOpenSerial.IsEnabled = true;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
