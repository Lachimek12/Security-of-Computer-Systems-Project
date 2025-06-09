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
using MainApp.ViewModels;

namespace MainApp
{
    /// <summary>
    /// Main window handling navigation between Sign and Verify views.
    /// Implements the <see cref="Window"/> interface.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes the window and loads the Sign view.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.Sign_Clicked(null, null);
        }

        /// <summary>
        /// Switches to the Sign view.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Sign_Clicked(object sender, RoutedEventArgs e)
        {
            this.Width = 400;
            this.Height = 360;
            this.ResizeMode = ResizeMode.CanResize;
            SignNavbarButton.Visibility = Visibility.Collapsed;
            VerifyNavbarButton.Visibility = Visibility.Visible;
            
            DataContext = new SignViewModel();
        }

        /// <summary>
        /// Switches to the Verify view.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Verify_Clicked(object sender, RoutedEventArgs e)
        {
            this.Width = 400;
            this.Height = 360;
            this.ResizeMode = ResizeMode.CanResize;
            VerifyNavbarButton.Visibility = Visibility.Collapsed;
            SignNavbarButton.Visibility = Visibility.Visible;
            DataContext = new VerifyViewModel();
        }
    }
}
