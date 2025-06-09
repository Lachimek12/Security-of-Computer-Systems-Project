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
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using Org.BouncyCastle.Math;
using iTextSharp.text;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.X509;
using System.Windows.Documents;
using Path = System.IO.Path;


namespace MainApp.Models
{
    /// <summary>
    /// Provides methods to sign PDF files and verify digital signatures.
    /// </summary>
    internal class SignModel
    {
        /// <summary>
        /// Length of AES encryption key in bits.
        /// </summary>
        private readonly int _AESkeyLength = 256;

        /// <summary>
        /// Initialization vector for AES encryption/decryption.
        /// </summary>
        private readonly byte[] _iv = new byte[]
        {
            0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF,
            0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10
        };

        /// <summary>
        /// Signs a PDF file using the provided public and private keys and a passphrase.
        /// </summary>
        /// <param name="publicKey">Path to the public certificate file (.cer).</param>
        /// <param name="privateKey">Path to the encrypted private key file.</param>
        /// <param name="pdfFilePath">Path to the PDF file to sign.</param>
        /// <param name="input">Passphrase used to decrypt the private key.</param>
        /// <exception cref="Exception">Thrown when the private key decryption fails.</exception>
        public void SignPdf(string publicKey, string privateKey, string pdfFilePath, string input)
        {            
            var cert = new X509Certificate2(publicKey);
            byte[] data = File.ReadAllBytes(privateKey);
            byte[] hashedInput = HashInput(input);

            System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create();
            aes.KeySize = _AESkeyLength;
            aes.Key = hashedInput;
            aes.IV = _iv;
            aes.Mode = CipherMode.CFB;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            byte[] decyptedData = null;
            try
            {
                decyptedData = decryptor.TransformFinalBlock(data, 0, data.Length);
            }
            catch(Exception ex)
            {
                throw new Exception("Not valid password", ex);
            }
            var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(decyptedData, out _);
            var bcCert = DotNetUtilities.FromX509Certificate(cert);

            using var reader = new PdfReader(pdfFilePath);
            string signedPdfFilePath = Path.Combine(
                Path.GetDirectoryName(pdfFilePath),
                Path.GetFileNameWithoutExtension(pdfFilePath) + "_signed" + Path.GetExtension(pdfFilePath)
            );
            using var output = new FileStream(signedPdfFilePath, FileMode.Create);
            var stamper = PdfStamper.CreateSignature(reader, output, '\0');

            var appearance = stamper.SignatureAppearance;
            appearance.SetVisibleSignature(new iTextSharp.text.Rectangle(100, 100, 250, 150), 1, "Signature1");
            appearance.Reason = "PAdES test";
            appearance.Location = "MySystem";

            RSAParameters rsaParams = rsa.ExportParameters(true);
            var modulus = new BigInteger(1, rsaParams.Modulus);
            var privateExponent = new BigInteger(1, rsaParams.D);

            // Create the RSA private key for BouncyCastle
            RsaKeyParameters privateKeyParameters = new RsaKeyParameters(true, modulus, privateExponent);


            var externalSignature = new PrivateKeySignature(privateKeyParameters, "SHA-256");

            MakeSignature.SignDetached(
                appearance,
                externalSignature,
                new[] { bcCert },
                null, null, null,
                0,
                CryptoStandard.CMS);
        }

        /// <summary>
        /// Computes the SHA-256 hash of the given input string.
        /// </summary>
        /// <param name="input">Input string to hash.</param>
        /// <returns>Hashed bytes.</returns>
        private byte[] HashInput(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return hashBytes;
            }
        }

        /// <summary>
        /// Verifies that the signatures in a signed PDF are valid and match the provided public key.
        /// </summary>
        /// <param name="publicKey">Path to the public certificate file (.cer).</param>
        /// <param name="signedPdf">Path to the signed PDF file.</param>
        /// <exception cref="InvalidOperationException">Thrown when signature verification fails or signatures are invalid.</exception>
        public void VerifyKeys(string publicKey, string signedPdf)
        {
            using var publicKeyStream = new FileStream(publicKey, FileMode.Open);
            var parser = new X509CertificateParser();
            var certificate = parser.ReadCertificate(publicKeyStream);
            publicKeyStream.Dispose();

            PdfReader reader = new PdfReader(signedPdf);
            AcroFields af = reader.AcroFields;
            var names = af.GetSignatureNames();

            if (names.Count == 0)
            {
                throw new InvalidOperationException("No Signature present in pdf file.");
            }

            foreach (string name in names)
            {
                if (!af.SignatureCoversWholeDocument(name))
                {
                    throw new InvalidOperationException(string.Format($"The signature: {0} does not covers the whole document.", name));
                }

                PdfPKCS7 pk = af.VerifySignature(name);
                var cal = pk.SignDate;
                var pkc = pk.Certificates;

                if (!pk.Verify())
                {
                    throw new InvalidOperationException("The signature could not be verified.");
                }

                IList<VerificationException> fails = CertificateVerification.VerifyCertificates(pkc, new Org.BouncyCastle.X509.X509Certificate[] { certificate }, null, cal);
                if (fails.Count > 0)
                {
                    throw new InvalidOperationException("The file is not signed using the specified key.");
                }
            }
        }
    }

}
