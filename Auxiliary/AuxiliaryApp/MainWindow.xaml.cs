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
    /// Main window handling the primary view.
    /// Implements the <see cref="Window"/> interface.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes the window and loads the main view.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.Main_Clicked(null, null);
        }

        /// <summary>
        /// Sets window properties and loads MainViewModel.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
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
