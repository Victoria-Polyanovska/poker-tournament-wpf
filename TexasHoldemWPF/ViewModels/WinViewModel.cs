using System.Threading.Tasks;
using System.Windows;
using TexasHoldemWPF.Resources;
using TexasHoldemWPF.Services;
using TexasHoldemWPF.Views;

namespace TexasHoldemWPF.ViewModels
{
    public class WinViewModel : BaseViewModel
    {
        private readonly NavigationService _navigationService;

        public string Message { get; } = "You won!";
        public string SubMessage { get; } = "But remember, that the house always wins";

        public WinViewModel(NavigationService navigationService, int prize)
        {
            _navigationService = navigationService;

            var persistence = new BalancePersistenceService();
            var balance = persistence.Load();
            persistence.Save(balance + prize);

            Task.Delay(5000).ContinueWith(_ =>
                Application.Current.Dispatcher.Invoke(() =>
                    _navigationService.NavigateTo<MenuView>(
                        new MenuViewModel(_navigationService))));
        }
    }
}