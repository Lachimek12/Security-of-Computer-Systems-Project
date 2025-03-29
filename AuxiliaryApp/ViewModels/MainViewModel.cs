using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AuxiliaryApp.Commands;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Intrinsics.X86;
using System.Printing;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;
using System.Windows.Media.Media3D;

namespace AuxiliaryApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Consturctors
        public MainViewModel()
        {

            // this.ExampleCommand = new RelayCommand(() => {});


        }

        #endregion


        #region Members


        #endregion


        #region Properties

        /*
        public string Example
        {
            get { return this._example; }
            set
            {
                if (value != this._example)
                {
                    this._example = value;
                    RaisePropertyChanged(nameof(Example));
                }
            }
        }
        */



        // public ICommand ExampleCommand { get; }


        #endregion


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion


        #region Methods

        #endregion
    }

}
