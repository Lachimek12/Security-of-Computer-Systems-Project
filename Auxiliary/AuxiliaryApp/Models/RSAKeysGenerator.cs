using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
            _publicKeyFilePath = keysDirectory + "/public.dat";
            _privateKeyFilePath = keysDirectory + "/private.dat";
        }
        public void GenerateKeys(string input)
        {
            RSA rsa = RSA.Create(_RSAkeysLength);
            var parameters = rsa.ExportParameters(true);


            byte[] hashedInput = HashInput(input);

            Aes aes = Aes.Create();
            aes.KeySize = _AESkeyLength;
            aes.Key = hashedInput;
            aes.IV = _iv;
            aes.Mode = CipherMode.CFB;



            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            byte[] encryptedPrivateKey = encryptor.TransformFinalBlock(parameters.D, 0, parameters.D.Length);

            SaveKeysToFile(encryptedPrivateKey, parameters.Modulus);
        }


        private void SaveKeysToFile(byte[] privateKey, byte[] publicKey)
        {
            File.WriteAllBytes(_privateKeyFilePath, privateKey);
            File.WriteAllBytes(_publicKeyFilePath, publicKey);
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
