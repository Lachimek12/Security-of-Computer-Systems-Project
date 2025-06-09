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
using MainApp.Commands;
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
using MainApp.Models;
using System.Management;

namespace MainApp.ViewModels
{
    // <summary>
    /// ViewModel to manage PDF signing with PIN and private key detection.    
    /// Implements the <see cref="INotifyPropertyChanged"/> interface.
    /// </summary>
    public class SignViewModel : INotifyPropertyChanged
    {
        #region  Type Definitions
        /// <summary>
        /// Status states for UI feedback.
        /// </summary>
        enum Status
        {
            WaitingForPin,      
            PdfValid,
            WaitingForPdf,               
            PdfAndPinValid,
            PdfSigning,
            PdfSigned,
            PrivateKeyNotFound,
            SignUnsuccesful
        }

        #endregion


        #region Consturctors

        /// <summary>
        /// Initializes a new instance of the <see cref="SignViewModel"/> class.
        /// Sets initial status, commands, tries to find private key, and starts drive watcher.
        /// </summary>
        public SignViewModel()
        {
            _statusMessages = new Dictionary<Status, string>()
            {
                {Status.WaitingForPin, "Waiting for PIN" },
                {Status.PdfValid, "Waiting for valid PIN" },
                {Status.WaitingForPdf, "Waiting for pdf file"},
                {Status.PdfAndPinValid, "Ready to sign PDF" },
                {Status.PdfSigning, "Pdf is being signed" },
                {Status.PdfSigned, "Pdf file signed succesfully in the same directory" },
                {Status.PrivateKeyNotFound, "Couldn't find private key" },
                {Status.SignUnsuccesful, "Couldn't sign pdf" }
            };

            Button_IsSigningEnabled = "false";
            SetStatusLabel(Status.WaitingForPin);

            ChoosePdfFileCommand = new RelayCommand(ChoosePdfFile);
            SignPdfFileCommand = new RelayCommand(SignPdfFile);
            try
            {
                this._privateKeyFilePath = ExtractPrivateKey();
            }
            catch (Exception) { };
            StartDriveWatcher();

        }

        #endregion


        #region Members

        private SignModel _signModel = new();

        private readonly Dictionary<Status, string> _statusMessages;
        private string _pdfFilePath;
        private string _privateKeyFilePath;

        private string _isSigningEnabled;
        private string _pin;
        private string _pdfFileName;
        private string _status;

        #endregion


        #region Properties
        /// <summary>
        /// PIN input bound to UI.
        /// </summary>
        public string TextBox_PIN
        {
            get { return this._pin; }
            set
            {
                Button_IsSigningEnabled = "false";

                if (IsValidPINInput(value))
                {
                    this._pin = value;
                    RaisePropertyChanged(nameof(TextBox_PIN));
                }

                bool isValidPIN = IsValidPIN();

                ReadyToSign();
            }
        }

        /// <summary>
        /// PDF filename displayed in UI.
        /// </summary>
        public string Label_PdfFileName
        {
            get { return this._pdfFileName; }
            set
            {
                this._pdfFileName = value;
                RaisePropertyChanged(nameof(Label_PdfFileName));
            }
        }
        /// <summary>
        /// Status message shown to the user.
        /// </summary>
        public string Label_Status
        {
            get { return this._status; }
            set
            {
                this._status = value;
                RaisePropertyChanged(nameof(Label_Status));
            }
        }

        /// <summary>
        /// Controls if sign button is enabled.
        /// </summary>
        public string Button_IsSigningEnabled
        {
            get { return this._isSigningEnabled; }
            set
            {
                this._isSigningEnabled = value;
                RaisePropertyChanged(nameof(Button_IsSigningEnabled));
            }
        }

        /// <summary>
        /// Command to open PDF file dialog.
        /// </summary>
        public ICommand ChoosePdfFileCommand { get; }

        /// <summary>
        /// Command to start signing the selected PDF.
        /// </summary>
        public ICommand SignPdfFileCommand { get; }


        #endregion


        #region INotifyPropertyChanged Members

        /// <summary>
        /// Event triggered when a property changes.
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
        /// Updates the status label with a custom message.
        /// </summary>
        /// <param name="status">The custom status message.</param>
        private void SetCutomStatusLabel(string status)
        {
            Label_Status = status;
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
            if (_pin == null) return false;

            string pattern = @"^\d{6}$";

            return Regex.IsMatch(_pin, pattern);
        }

        /// <summary>
        /// Signs the PDF using the private key and PIN.
        /// </summary>
        private void SignPdfFile()
        {
            SetStatusLabel(Status.PdfSigning);
            try
            {
                string certificatePath = ExtractCertificate();

                _signModel.SignPdf(certificatePath, this._privateKeyFilePath, this._pdfFilePath, this._pin);
                SetStatusLabel(Status.PdfSigned);
            }
            catch (Exception ex)
            {
                SetCutomStatusLabel(ex.Message);
            }
        }

        /// <summary>
        /// Opens a file dialog to choose a PDF file.
        /// </summary>
        private void ChoosePdfFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
            openFileDialog.Title = "Select a PDF file";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                this._pdfFilePath = openFileDialog.FileName;
                this.Label_PdfFileName = Regex.Match(this._pdfFilePath, @"[^\\\/]+$").Value;

                ReadyToSign();
            }
        }

        /// <summary>
        /// Searches removable drives for private key file "private.dat".
        /// </summary>
        /// <returns>Path to private key file.</returns>
        /// <exception cref="Exception">If private key not found.</exception>
        private string ExtractPrivateKey()
        {
            string privateKeyFile = "private.dat";

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Removable && drive.IsReady)
                {
                    string[] files = Directory.GetFiles(drive.RootDirectory.FullName, privateKeyFile, SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        return files[0];
                    }
                }
            }

            throw new Exception("private.dat file wasn't found");
        }

        /// <summary>
        /// Opens a file dialog to select a certificate (.cer) file.
        /// </summary>
        /// <returns>Path to certificate file.</returns>
        /// <exception cref="InvalidDataException">If invalid file selected.</exception>
        private string ExtractCertificate()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Certificate  Files (*.cer)|*.cer";
            openFileDialog.Title = "Select a Cer file";
            openFileDialog.Multiselect = false;
            string certificateFilePath = "";


            if (openFileDialog.ShowDialog() == true)
            {
                certificateFilePath = openFileDialog.FileName;
                return certificateFilePath;
            }
            else
            {
                throw new InvalidDataException("Not valid cer file");
            }

        }

        /// <summary>
        /// Updates UI status and button enable state based on inputs and key presence.
        /// </summary>
        private void ReadyToSign()
        {
            Button_IsSigningEnabled = "false";

            if (!IsValidPIN())
            {
                SetStatusLabel(Status.WaitingForPin);
            }
            else if (this.Label_PdfFileName == null)
            {
                SetStatusLabel(Status.WaitingForPdf);
            }
            else if (this._privateKeyFilePath == null)
            {
                SetStatusLabel(Status.PrivateKeyNotFound);
            }
            else
            {
                SetStatusLabel(Status.PdfAndPinValid);
                Button_IsSigningEnabled = "true";
            }
        }

        /// <summary>
        /// Starts monitoring drives for private key availability changes.
        /// </summary>
        private void StartDriveWatcher()
        {
            var watcher = new ManagementEventWatcher(
                new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent"));
            watcher.EventArrived += (s, e) => UpdateDrives();
            watcher.Start();
        }

        /// <summary>
        /// Updates private key path and UI when drives change.
        /// </summary>
        private void UpdateDrives()
        {
            try
            {
                this._privateKeyFilePath = ExtractPrivateKey();
            }
            catch (Exception ex)
            {
                this._privateKeyFilePath = null;
            }
            ReadyToSign();
        }

        #endregion


        #region Service Methods

        /// <summary>
        /// Raises PropertyChanged event for UI updates.
        /// </summary>
        /// <param name="propertyName">Changed property name.</param>
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
