using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AuxiliaryApp.Models
{
    class RSAKeysGenerator
    {
        private readonly int _RSAkeysLength = 4096;
        private readonly int _AESkeyLength = 256;
        private readonly byte[] _iv = new byte[]
        {
            0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF,
            0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10
        };

        private readonly string _publicKeyFilePath;
        private readonly string _privateKeyFilePath;

        public RSAKeysGenerator()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            string keysDirectory = projectDirectory + "/Assets";
            _publicKeyFilePath = keysDirectory + "/public.cer";
            _privateKeyFilePath = keysDirectory + "/private.dat";
        }
        public void GenerateKeys(string input)
        {
            RSA rsa = RSA.Create(_RSAkeysLength);

            var req = new CertificateRequest("CN=Signer", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
            var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(10));

            // Save public cert
            File.WriteAllBytes(_publicKeyFilePath, cert.Export(X509ContentType.Cert));

            var parameters = rsa.ExportParameters(true);


            byte[] hashedInput = HashInput(input);

            Aes aes = Aes.Create();
            aes.KeySize = _AESkeyLength;
            aes.Key = hashedInput;
            aes.IV = _iv;
            aes.Mode = CipherMode.CFB;


            byte[] privateKey = rsa.ExportRSAPrivateKey();

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            byte[] encryptedPrivateKey = encryptor.TransformFinalBlock(privateKey, 0, privateKey.Length);

            SaveKeysToFile(encryptedPrivateKey, parameters.Modulus);
        }


        private void SaveKeysToFile(byte[] privateKey, byte[] publicKey)
        {
            File.WriteAllBytes(_privateKeyFilePath, privateKey);
            //File.WriteAllBytes(_publicKeyFilePath, publicKey);
        }

        private byte[] HashInput(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return hashBytes;
            }
        }

    }
}
