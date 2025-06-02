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
    /// Interaction logic for RedView.xaml
    /// </summary>
    public partial class SignView : UserControl
    {

        #region Constructors
        public SignView()
        {
            InitializeComponent();
            DataContext = new SignViewModel();
            _viewModel = (SignViewModel)DataContext;
        }

        #endregion


        #region Members

        private SignViewModel _viewModel;

        #endregion

    }
}
