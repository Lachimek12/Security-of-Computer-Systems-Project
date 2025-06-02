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
    public class SignViewModel : INotifyPropertyChanged
    {
        #region  Type Definitions
        enum Status
        {
            WaitingForPin,      
            PdfValid,                
            PinValid,               
            PdfAndPinValid,
            PdfSigning,
            PdfSigned,
            SignUnsuccesful
        }

        #endregion


        #region Consturctors
        public SignViewModel()
        {
            _statusMessages = new Dictionary<Status, string>()
            {
                {Status.WaitingForPin, "Waiting for PIN" },
                {Status.PdfValid, "Waiting for valid PIN" },
                {Status.PinValid, "Waiting for pdf file"},
                {Status.PdfAndPinValid, "Ready to sign PDF" },
                {Status.PdfSigning, "Pdf is being signed" },
                {Status.PdfSigned, "Pdf file signed succesfully in the same directory" },
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
            catch (Exception ex) { };
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

                if (isValidPIN && this.Label_PdfFileName != null)
                {
                    Button_IsSigningEnabled = "true";
                    SetStatusLabel(Status.PdfAndPinValid);
                }
                else if (isValidPIN)
                {
                    SetStatusLabel(Status.PinValid);
                }
                else
                {
                    SetStatusLabel(Status.WaitingForPin);
                }
            }
        }

        public string Label_PdfFileName
        {
            get { return this._pdfFileName; }
            set
            {
                this._pdfFileName = value;
                RaisePropertyChanged(nameof(Label_PdfFileName));
            }
        }
        public string Label_Status
        {
            get { return this._status; }
            set
            {
                this._status = value;
                RaisePropertyChanged(nameof(Label_Status));
            }
        }

        public string Button_IsSigningEnabled
        {
            get { return this._isSigningEnabled; }
            set
            {
                this._isSigningEnabled = value;
                RaisePropertyChanged(nameof(Button_IsSigningEnabled));
            }
        }


        public ICommand ChoosePdfFileCommand { get; }
        
        public ICommand SignPdfFileCommand { get; }


        #endregion


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion


        #region Util Methods
        private void SetCutomStatusLabel(string status)
        {
            Label_Status = status;
        }
        private void SetStatusLabel(Status status)
        {
            Label_Status = _statusMessages[status];
        }

        private bool IsValidPINInput(string pinInput)
        {
            string pattern = @"^\d{0,6}$";

            return Regex.IsMatch(pinInput, pattern);
        }

        private bool IsValidPIN()
        {
            if (_pin == null) return false;

            string pattern = @"^\d{6}$";

            return Regex.IsMatch(_pin, pattern);
        }

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
                SetStatusLabel(Status.SignUnsuccesful);
            }
        }

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
                if (this.Label_PdfFileName != null && IsValidPIN())
                {
                    SetStatusLabel(Status.PdfAndPinValid);
                    Button_IsSigningEnabled = "true";
                }
                else if(this.Label_PdfFileName != null)
                {
                    SetStatusLabel(Status.PdfValid);
                }
                else
                {
                    Button_IsSigningEnabled = "false";
                }
            }
        }

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

        private void StartDriveWatcher()
        {
            var watcher = new ManagementEventWatcher(
                new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent"));
            watcher.EventArrived += (s, e) => UpdateDrives();
            watcher.Start();
        }

        private void UpdateDrives()
        {
            try
            {
                this._privateKeyFilePath = ExtractPrivateKey();
                SetCutomStatusLabel("Private key found on USB drive");
            }
            catch (Exception ex)
            {
                SetCutomStatusLabel(ex.Message);
            }
        }

        #endregion


        #region Service Methods

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
