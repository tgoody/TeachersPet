using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TeachersPet.Services {
    public class EncryptService {
        /// <summary>
        /// Create and initialize a crypto algorithm.
        /// </summary>
        /// <param name="password">The password.</param>
        private static SymmetricAlgorithm GetAlgorithm(string password)
        {
            var algorithm = Rijndael.Create();
            //TODO: change this salt
            var rdb = new Rfc2898DeriveBytes(password, new byte[] {
                0x53,0x6f,0x64,0x69,0x75,0x6d,0x20,             // salty goodness
                0x43,0x68,0x6c,0x6f,0x72,0x69,0x64,0x65
            });
            algorithm.Padding = PaddingMode.ISO10126;
            algorithm.Key = rdb.GetBytes(32);
            algorithm.IV = rdb.GetBytes(16);
            return algorithm;
        }


        /// <summary>
        /// Encrypts byte array with a given password.
        /// </summary>
        /// <param name="data">The byte array of data.</param>
        /// <param name="password">The password.</param>
        public static byte[] EncryptData(byte[] data, string password)
        {
            var algorithm = GetAlgorithm(password);
            var encryptor = algorithm.CreateEncryptor();
            var clearBytes = data;
            var ms = new MemoryStream();
            var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            cs.Write(clearBytes, 0, clearBytes.Length);
            cs.Close();
            return ms.ToArray();
        }

        /// <summary>
        /// Decrypts data using a given password.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="password">The password.</param>
        public static byte[] DecryptData(byte[] data, string password)
        {
            var algorithm = GetAlgorithm(password);
            var decryptor = algorithm.CreateDecryptor();
            var cipherBytes = data;
            var ms = new MemoryStream();
            var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
            cs.Write(cipherBytes, 0, cipherBytes.Length);
            cs.Close();
            return ms.ToArray();
        }
    }
}