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

namespace AuxiliaryApp.Views
{
    /// <summary>
    /// Interaction logic for the main user interface view.
    /// Binds to <see cref="MainViewModel"/>.
    /// Implements the <see cref="UserControl"/> interface.
    /// </summary>
    public partial class MainView : UserControl
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainView"/> class.
        /// Sets the DataContext to a new <see cref="MainViewModel"/> instance.
        /// </summary>
        public MainView()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            _viewModel = (MainViewModel)DataContext;
        }

        #endregion


        #region Members

        /// <summary>
        /// Reference to the ViewModel backing this view.
        /// </summary>
        private MainViewModel _viewModel;

        #endregion

    }
}
