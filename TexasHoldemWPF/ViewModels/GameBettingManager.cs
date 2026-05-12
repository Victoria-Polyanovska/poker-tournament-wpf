using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TexasHoldemWPF.Enums;
using TexasHoldemWPF.Models.Entities;
using TexasHoldemWPF.Models.Actions;

namespace TexasHoldemWPF.ViewModels
{
    public class GameBettingManager
    {
        private readonly GameViewModel _context;
        private bool _raiseOccurredInRound = false;

        private const double HALF_POT = 0.5;
        private const double TWO_THIRDS_POT = 0.666;
        private const double THREE_QUARTERS_POT = 0.75;
        private const double RAISE_MULTIPLIER_2 = 2;
        private const double RAISE_MULTIPLIER_3 = 3;
        private const double RAISE_MULTIPLIER_4 = 4;
        private const double BIG_BLIND_MULTIPLIER_2_5 = 2.5;
        private const double BIG_BLIND_MULTIPLIER_4 = 4;
        private const double BIG_BLIND_MULTIPLIER_6 = 6;

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
            new RaiseAction(betAmount).Execute(_context, _context.GetPlayer(0));
            _context.UpdateCallCheckButtonText();
            _ = _context.BotManager.StartBotActions();
        }

        public void ShowRaiseMenu()
        {
            if (!_context.CanAct) return;

            if (_context.CurrentBet == 0)
            {
                _context.RaiseOptions = new List<int> {
                    (int)(_context.BigBlindAmount * BIG_BLIND_MULTIPLIER_2_5),
                    _context.BigBlindAmount * (int)BIG_BLIND_MULTIPLIER_4,
                    _context.BigBlindAmount * (int)BIG_BLIND_MULTIPLIER_6,
                    _context.PlayerBalance
                };
                _context.RaiseOptionsText = "2.5BB|4BB|6BB|ALL IN";
            }
            else
            {
                if (_context.CurrentBet == 0)
                {
                    _context.RaiseOptions = new List<int> {
                        (int)(_context.PotSize * HALF_POT),
                        (int)(_context.PotSize * TWO_THIRDS_POT),
                        (int)(_context.PotSize * THREE_QUARTERS_POT),
                        _context.PlayerBalance
                    };
                    _context.RaiseOptionsText = "1/2 POT|2/3 POT|3/4 POT|ALL IN";
                }
                else
                {
                    _context.RaiseOptions = new List<int> {
                        (int)(_context.CurrentBet * RAISE_MULTIPLIER_2),
                        (int)(_context.CurrentBet * RAISE_MULTIPLIER_3),
                        (int)(_context.CurrentBet * RAISE_MULTIPLIER_4),
                        _context.PlayerBalance
                    };
                    _context.RaiseOptionsText = "X2|X3|X4|ALL IN";
                }
            }

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
            new RaiseAction(raiseAmount).Execute(_context, _context.GetPlayer(0));

            if (_context.PlayerBalance == 0)
            {
                await HandleAllInRaise();
            }
            else
            {
                _raiseOccurredInRound = true;
                _context.UpdateCallCheckButtonText();
                _ = _context.BotManager.StartBotActions();
            }
        }

        private bool IsInvalidRaise(int raiseAmount)
        {
            return raiseAmount <= 0 || raiseAmount > _context.PlayerBalance;
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

        public void Call()
        {
            if (!_context.CanAct) return;

            new CallAction().Execute(_context, _context.GetPlayer(0));
            _context.UpdateCallCheckButtonText();
            _context.CanAct = false;
            _ = _context.BotManager.StartBotActions();
        }

        public void Fold()
        {
            if (!_context.CanAct) return;

            _context.CanAct = false;
            new FoldAction().Execute(_context, _context.GetPlayer(0));

            var activeBots = GetActiveBots();
            if (activeBots.Count == 1)
            {
                HandleSingleBotRemaining(activeBots.First());
                return;
            }

            _context.WinnerManager.DetermineWinnerAfterFold();
        }

        private List<Player> GetActiveBots()
        {
            return _context.Players.Where(p => !p.IsFolded && p is BotPlayer).ToList();
        }

        private void HandleSingleBotRemaining(Player winner)
        {
            winner.Balance += _context.PotSize;
            _context.ShowGameMessage($"{winner.Name} wins ${_context.PotSize} (all others folded)");
            _context.RoundManager.StartNewRound();
        }

        public bool RaiseOccurredInRound => _raiseOccurredInRound;
        public void SetRaiseOccurred(bool value) => _raiseOccurredInRound = value;
    }
}