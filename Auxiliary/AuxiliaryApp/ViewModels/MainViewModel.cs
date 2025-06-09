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
using AuxiliaryApp.Models;

namespace AuxiliaryApp.ViewModels
{
    /// <summary>
    /// ViewModel for the main application logic, handling PIN validation and RSA key generation.
    /// Implements the <see cref="INotifyPropertyChanged"/> interface.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Application status states.
        /// </summary>
        #region  Type Definitions
        enum Status
        {
            WaitingForPIN,
            CorrectPin,
            IncorrectPin,
            GeneratingKeys,
            KeysGenerated
        }

        #endregion

        #region Consturctors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            _statusMessages = new Dictionary<Status, string>()
            {
                {Status.WaitingForPIN, "Waiting for PIN" },
                {Status.CorrectPin, "Ready to generate keys"},
                {Status.IncorrectPin, "PIN not valid" },
                {Status.GeneratingKeys, "RSA keys are generating" },
                {Status.KeysGenerated, "Keys succesfuly generated and encrypted" }
            };

            SetStatusLabel(Status.WaitingForPIN);

            this.GenerateKeysCommand = new RelayCommand(
                () => {
                    if (IsValidPIN()) {
                        SetStatusLabel(Status.GeneratingKeys);
                        GenerateKeys();
                        SetStatusLabel(Status.KeysGenerated);
                    }
                    else SetStatusLabel(Status.IncorrectPin);
                }
            );


        }

        #endregion


        #region Members

        private RSAKeysGenerator _rsaKeysGenerator = new();

        private readonly Dictionary<Status, string> _statusMessages;

        private string _pin;
        private string _status;

        #endregion


        #region Properties
        /// <summary>
        /// Gets or sets the PIN entered by the user.
        /// </summary>
        public string TextBox_PIN
        {
            get { return this._pin; }
            set
            {
                if (IsValidPINInput(value))
                {
                    this._pin = value;
                    RaisePropertyChanged(nameof(TextBox_PIN));
                }

                if (IsValidPIN()) 
                {
                    SetStatusLabel(Status.CorrectPin); 
                }
                else
                {
                    SetStatusLabel(Status.WaitingForPIN);
                }
            }
        }

        /// <summary>
        /// Gets or sets the current status label message.
        /// </summary>
        public string Label_Status
        {
            get { return this._status; }
            set
            {
                _status = value;
                RaisePropertyChanged(nameof(Label_Status));
            }
        }

        /// <summary>
        /// Gets the command that triggers RSA key generation.
        /// </summary>
        public ICommand GenerateKeysCommand { get; }

        #endregion


        #region INotifyPropertyChanged Members

        /// <summary>
        /// Event triggered when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion


        #region Util Methods

        /// <summary>
        /// Sets the current status label based on the given status.
        /// </summary>
        /// <param name="status">The current status to display.</param>
        private void SetStatusLabel(Status status)
        {
            Label_Status = _statusMessages[status];
        }

        /// <summary>
        /// Validates PIN input format (0 to 6 digits).
        /// </summary>
        /// <param name="pinInput">The PIN input string.</param>
        /// <returns>True if input format is valid; otherwise, false.</returns>
        private bool IsValidPINInput(string pinInput)
        {
            string pattern = @"^\d{0,6}$";

            return Regex.IsMatch(pinInput, pattern);
        }

        /// <summary>
        /// Validates if the entered PIN is exactly 6 digits.
        /// </summary>
        /// <returns>True if PIN is valid; otherwise, false.</returns>
        private bool IsValidPIN()
        {
            if(_pin == null) return false;
            
            string pattern = @"^\d{6}$";

            return Regex.IsMatch(_pin, pattern);
        }

        /// <summary>
        /// Triggers RSA key generation using the current PIN.
        /// </summary>
        private void GenerateKeys()
        {
            _rsaKeysGenerator.GenerateKeys(_pin);
        }

        #endregion

        #region Service Methods

        /// <summary>
        /// Raises the PropertyChanged event for the given property.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }

}
