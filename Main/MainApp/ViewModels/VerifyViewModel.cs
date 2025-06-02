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

namespace MainApp.ViewModels
{
    public class VerifyViewModel : INotifyPropertyChanged
    {
        #region  Type Definitions
        enum Status
        {
            WaitingForPdf,
            PdfValid,
            VeryfingPdf,
            PdfVerified,
        }

        #endregion

        #region Consturctors
        public VerifyViewModel()
        {
            _statusMessages = new Dictionary<Status, string>()
            {         
                {Status.WaitingForPdf, "Waiting for PDF" },
                {Status.PdfValid, "Ready to verify PDF"},
                {Status.VeryfingPdf, "Veryfing PDF" },
                {Status.PdfVerified, "PDF file verified succesfuly" },
            };

            Button_IsVeryfingEnabled = "false";
            SetStatusLabel(Status.WaitingForPdf);

            ChoosePdfFileCommand = new RelayCommand(ChoosePdfFile);
            VerifyPdfFileCommand = new RelayCommand(VerifyPdfFile);
        }

        #endregion


        #region Members

        private SignModel _signModel = new();

        private readonly Dictionary<Status, string> _statusMessages;
        private string _pdfFilePath;

        private string _isVeryfingEnabled;
        private string _pdfFileName;
        private string _status;

        #endregion


        #region Properties

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

        public string Button_IsVeryfingEnabled
        {
            get { return this._isVeryfingEnabled; }
            set
            {
                this._isVeryfingEnabled = value;
                RaisePropertyChanged(nameof(Button_IsVeryfingEnabled));
            }
        }

        public ICommand ChoosePdfFileCommand { get; }

        public ICommand VerifyPdfFileCommand { get; }


        #endregion


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion


        #region Util Methods
        private void SetStatusLabel(Status status)
        {
            Label_Status = _statusMessages[status];
        }

        private void SetCutomStatusLabel(string status)
        {
            Label_Status = status;
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

        private void VerifyPdfFile()
        {
            SetStatusLabel(Status.VeryfingPdf);
            try
            {
                string certificatePath = ExtractCertificate();

                _signModel.VerifyKeys(certificatePath, this._pdfFilePath);
            }
            catch (Exception ex)
            {
                SetCutomStatusLabel(ex.Message);
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
                if (this.Label_PdfFileName != null)
                {
                    SetStatusLabel(Status.PdfValid);
                    Button_IsVeryfingEnabled = "true";
                }
                else
                {
                    Button_IsVeryfingEnabled = "false";
                }
            }
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

        #endregion

    }

}
