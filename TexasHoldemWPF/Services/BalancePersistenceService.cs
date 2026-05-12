using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace TexasHoldemWPF.Services
{
    public class BalancePersistenceService
    {
        private const string BalanceFile = "playerBalance.json";
        private const double DefaultBalance = 10000;

        private static readonly byte[] AesKey = Encoding.UTF8.GetBytes("k9H2pLm4Wq8ZxRt7");
        private static readonly byte[] AesIV = Encoding.UTF8.GetBytes("J3mN7vBt1QsPfYl9");

        public double Load()
        {
            if (!File.Exists(BalanceFile))
                return DefaultBalance;

            var encrypted = File.ReadAllText(BalanceFile);
            var decrypted = Decrypt(encrypted);
            return JsonConvert.DeserializeObject<double>(decrypted);
        }

        public void Save(double balance)
        {
            var json = JsonConvert.SerializeObject(balance);
            var encrypted = Encrypt(json);
            File.WriteAllText(BalanceFile, encrypted);
        }

        private static string Encrypt(string plainText)
        {
            var aes = Aes.Create();
            aes.Key = AesKey;
            aes.IV = AesIV;
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        private static string Decrypt(string cipherText)
        {
            var buffer = Convert.FromBase64String(cipherText);
            var aes = Aes.Create();
            aes.Key = AesKey;
            aes.IV = AesIV;
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            var ms = new MemoryStream(buffer);
            var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}