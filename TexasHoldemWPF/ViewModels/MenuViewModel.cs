using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TexasHoldemWPF.Models.Entities;
using TexasHoldemWPF.Models.Factory;
using TexasHoldemWPF.Models.Factory.BotFactories;
using TexasHoldemWPF.Resources;
using TexasHoldemWPF.Services;
using TexasHoldemWPF.Views;

namespace TexasHoldemWPF.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly NavigationService _navigationService;
        private readonly BalancePersistenceService _persistence = new BalancePersistenceService();
        private double _playerBalance;

        public double PlayerBalance
        {
            get => _playerBalance;
            set
            {
                _playerBalance = value;
                OnPropertyChanged();
                _persistence.Save(value);   
            }
        }

        public ObservableCollection<Tournament> Tournaments { get; } = new ObservableCollection<Tournament>
        {
            new Tournament("Paris",         30_000,     10_000, "pack://application:,,,/Resources/Images/Paris.jpg"),
            new Tournament("Rio de Janeiro",250_000,    75_000, "pack://application:,,,/Resources/images/Rio.jpg"),
            new Tournament("Sydney",      1_500_000,   500_000, "pack://application:,,,/Resources/images/Sydney.jpg"),
            new Tournament("Tokyo",      10_000_000, 3_000_000, "pack://application:,,,/Resources/images/Tokyo.jpg")
        };

        public ICommand AddMoneyCommand { get; }
        public ICommand StartTournamentCommand { get; }
        public ICommand ShowRulesCommand { get; }

        public MenuViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
            _playerBalance = _persistence.Load();   

            AddMoneyCommand = new RelayCommand(AddMoney);
            StartTournamentCommand = new RelayCommand<Tournament>(StartTournament);
            ShowRulesCommand = new RelayCommand(ShowRules);
        }

        private void AddMoney() => PlayerBalance += 10_000;

        private void StartTournament(Tournament tournament)
        {
            if (PlayerBalance < tournament.BuyIn)
            {
                MessageBox.Show("Not enough money to join this tournament!",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PlayerBalance -= tournament.BuyIn;

            var factories = new List<IPlayerFactory>
            {
                new HumanPlayerFactory(),
                new BotAggresiveFactory(),
                new BotConservativeFactory(),
                new BotLooseFactory(),
                new BotTightFactory()
            };

            var gameViewModel = new GameViewModel(tournament, _navigationService, factories);
            _navigationService.NavigateTo(new GameView { DataContext = gameViewModel });
        }

        private void ShowRules() => new PokerRulesWindow().Show();
    }
}