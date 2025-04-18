﻿using System;
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
    /// Interaction logic for RedView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {

        #region Constructors
        public MainView()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            _viewModel = (MainViewModel)DataContext;
        }

        #endregion


        #region Members

        private MainViewModel _viewModel;

        #endregion

    }
}
