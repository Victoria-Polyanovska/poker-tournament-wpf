using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TexasHoldemWPF.Enums;
using TexasHoldemWPF.Models.Entities;
using TexasHoldemWPF.Models.Strategies; 

namespace TexasHoldemWPF.ViewModels
{
    public class GameBettingManager
    {
        private readonly GameViewModel _context;
        private bool _raiseOccurredInRound = false;

        public GameBettingManager(GameViewModel context)
        {
            _context = context;
        }

        public void StartBettingRound()
        {
            _raiseOccurredInRound = false;
            _context.UpdateCallCheckButtonText();
            _context.IsWaitingForPlayerAction = true;
            _context.CanAct = true;
        }

        public void Bet()
        {
            if (!_context.CanAct) return;

            int betAmount = Math.Min(GameViewModel.DEFAULT_BET_AMOUNT, _context.PlayerBalance);
            _context.CanAct = false;
            ExecuteBet(betAmount);
            _ = _context.BotManager.StartBotActions();
        }

        private void ExecuteBet(int amount)
        {
            UpdateBalances(amount);
            _context.CurrentBet = amount;
            UpdatePlayerAction(0, amount, $"Bet ${amount}");
        }

        public void ShowRaiseMenu()
        {
            if (!_context.CanAct) return;

            IRaiseOptionsStrategy strategy = _context.CurrentGameState == GameState.PreFlop
                ? new PreFlopRaiseStrategy()
                : new PostFlopRaiseStrategy();

            var result = strategy.GetOptions(_context);

            _context.RaiseOptions = result.Options;
            _context.RaiseOptionsText = result.Text;

            _context.UpdateCallCheckButtonText();
            _context.IsRaiseMenuVisible = true;
        }

        public async void ExecuteRaise(int raiseAmount)
        {
            if (IsInvalidRaise(raiseAmount))
            {
                _context.ShowGameMessage("Invalid raise amount");
                return;
            }

            _context.CanAct = false;
            ExecuteRaiseAmount(raiseAmount);

            if (_context.PlayerBalance == 0)
                await HandleAllInRaise();
            else
            {
                _raiseOccurredInRound = true;
                _context.UpdateCallCheckButtonText();
                _ = _context.BotManager.StartBotActions();
            }
        }

        private bool IsInvalidRaise(int raiseAmount) => raiseAmount <= 0 || raiseAmount > _context.PlayerBalance;

        private void ExecuteRaiseAmount(int raiseAmount)
        {
            UpdateBalances(raiseAmount);
            _context.CurrentBet += raiseAmount;
            UpdatePlayerAction(0, _context.CurrentBet, $"Raise ${raiseAmount}");
        }

        public void Call()
        {
            if (!_context.CanAct) return;

            int callAmount = Math.Min(CalculateCallAmount(), _context.PlayerBalance);

            if (callAmount == 0)
                _context.GetPlayer(0).LastAction = "Check";
            else
                ExecuteCall(callAmount);

            FinalizeCall();
        }

        private int CalculateCallAmount() => _context.CurrentBet - _context.GetPlayer(0).CurrentBet;

        private void ExecuteCall(int amount)
        {
            _context.GetPlayer(0).CurrentBet += amount;
            _context.PlayerBalance -= amount;
            _context.PotSize += amount;
            _context.GetPlayer(0).LastAction = $"Call ${amount}";
        }

        private void FinalizeCall()
        {
            _context.GetPlayer(0).Balance = _context.PlayerBalance;
            _context.UpdateCallCheckButtonText();
            _context.CanAct = false;
            _ = _context.BotManager.StartBotActions();
        }

        private void UpdateBalances(int amount)
        {
            _context.PlayerBalance -= amount;
            _context.PotSize += amount;
        }

        private void UpdatePlayerAction(int index, int bet, string actionText)
        {
            var player = _context.GetPlayer(index);
            player.Balance = _context.PlayerBalance;
            player.CurrentBet = bet;
            player.LastAction = actionText;
            _context.UpdateCallCheckButtonText();
        }
        public void Fold()
        {
            if (!_context.CanAct) return;
            _context.CanAct = false;
            _context.GetPlayer(0).IsFolded = true;
            _context.GetPlayer(0).LastAction = "Fold";

            var activeBots = GetActiveBots();
            if (activeBots.Count == 1)
            {
                HandleSingleBotRemaining(activeBots.First());
                return;
            }
            _context.WinnerManager.DetermineWinnerAfterFold();
        }

        private List<Player> GetActiveBots() => _context.Players.Where(p => !p.IsFolded && p is BotPlayer).ToList();

        private void HandleSingleBotRemaining(Player winner)
        {
            winner.Balance += _context.PotSize;
            _context.ShowGameMessage($"{winner.Name} wins ${_context.PotSize} (all others folded)");
            _context.RoundManager.StartNewRound();
        }

        private async Task HandleAllInRaise()
        {
            _context.SetEndedFlag(true);
            _context.UpdateCallCheckButtonText();
            _raiseOccurredInRound = true;
            await _context.BotManager.StartBotActions();
            _context.ShowAllCards = true;
            await FastForwardToShowdown();
        }

        private async Task FastForwardToShowdown()
        {
            while (_context.CommunityCards.Count < 5)
                _context.CommunityCards.Add(_context.CurrentDeck.DrawCard());

            _context.CurrentGameState = GameState.River;
            await Task.Delay(GameViewModel.STATE_TRANSITION_DELAY_MS * 10);
            _context.WinnerManager.EndGame();
        }

        public bool RaiseOccurredInRound => _raiseOccurredInRound;
        public void SetRaiseOccurred(bool value) => _raiseOccurredInRound = value;
    }
}