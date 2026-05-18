using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TexasHoldemWPF.Services
{
    public class AesCryptoProvider : ICryptoProvider
    {
        private readonly byte[] _aesKey;
        private readonly byte[] _aesIV;

        public AesCryptoProvider(string key = "k9H2pLm4Wq8ZxRt7", string iv = "J3mN7vBt1QsPfYl9")
        {
            _aesKey = Encoding.UTF8.GetBytes(key);
            _aesIV = Encoding.UTF8.GetBytes(iv);
        }

        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _aesKey;
            aes.IV = _aesIV;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string cipherText)
        {
            var buffer = Convert.FromBase64String(cipherText);
            using var aes = Aes.Create();
            aes.Key = _aesKey;
            aes.IV = _aesIV;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(buffer);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}
