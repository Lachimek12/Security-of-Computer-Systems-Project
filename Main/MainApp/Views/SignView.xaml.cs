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

namespace MainApp.Views
{
    /// <summary>
    /// Interaction logic for the main user interface view.
    /// Binds to <see cref="SignViewModel"/>.
    /// Implements the <see cref="UserControl"/> interface.
    /// </summary>
    public partial class SignView : UserControl
    {

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SignView"/> class.
        /// Sets the DataContext to a new <see cref="SignViewModel"/> instance.
        /// </summary>
        public SignView()
        {
            InitializeComponent();
            DataContext = new SignViewModel();
            _viewModel = (SignViewModel)DataContext;
        }

        #endregion


        #region Members

        /// <summary>
        /// Reference to the ViewModel backing this view.
        /// </summary>
        private SignViewModel _viewModel;

        #endregion

    }
}
