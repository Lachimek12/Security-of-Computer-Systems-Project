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
using AuxiliaryApp.ViewModels;

namespace AuxiliaryApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Main_Clicked(null, null);
        }

        private void Main_Clicked(object sender, RoutedEventArgs e)
        {
            this.Width = 400;
            this.Height = 360;
            this.ResizeMode = ResizeMode.NoResize;
            MainNavbarButton.Visibility = Visibility.Collapsed;
            DataContext = new MainViewModel();
        }
    }
}
