using System.IO;
using Newtonsoft.Json;

namespace TexasHoldemWPF.Services
{
    public class BalancePersistenceService
    {
        private const string BalanceFile = "playerBalance.json";
        private const double DefaultBalance = 10000;

        private readonly ICryptoProvider _cryptoProvider;

        // Новий конструктор, який приймає інтерфейс шифрування
        public BalancePersistenceService(ICryptoProvider cryptoProvider)
        {
            _cryptoProvider = cryptoProvider;
        }

        public double Load()
        {
            if (!File.Exists(BalanceFile))
                return DefaultBalance;

            var encrypted = File.ReadAllText(BalanceFile);
            var decrypted = _cryptoProvider.Decrypt(encrypted); // Викликаємо провайдер
            return JsonConvert.DeserializeObject<double>(decrypted);
        }

        public void Save(double balance)
        {
            var json = JsonConvert.SerializeObject(balance);
            var encrypted = _cryptoProvider.Encrypt(json); // Викликаємо провайдер
            File.WriteAllText(BalanceFile, encrypted);
        }
    }
}
