using System;
using System.Linq;
using System.Threading.Tasks;
using TexasHoldemWPF.Enums;
using TexasHoldemWPF.Models.Entities;
using TexasHoldemWPF.Models.Actions;

namespace TexasHoldemWPF.ViewModels
{
    public class GameBotManager
    {
        private readonly GameViewModel _context;

        public GameBotManager(GameViewModel context)
        {
            _context = context;
        }

        public async Task StartBotActions()
        {
            _context.IsWaitingForPlayerAction = false;
            bool newRaisesOccurred = false;

            for (int i = 0; i < _context.Players.Count; i++)
            {
                int currentIndex = (GameViewModel.BOT_START_INDEX + i) % _context.Players.Count;
                Player currentPlayer = _context.GetPlayer(currentIndex);

                if (ShouldSkipBot(currentIndex, currentPlayer))
                    continue;

                await Task.Delay(GameViewModel.BOT_ACTION_DELAY_MS);

                if (ShouldCheck(currentPlayer))
                {
                    currentPlayer.LastAction = "Check";
                    continue;
                }

                var action = ((BotPlayer)currentPlayer).MakeDecision(
                    _context.CurrentBet,
                    _context.PotSize,
                    _context.CommunityCards.ToList());

                var result = await ProcessBotAction(currentIndex, currentPlayer, action);

                if (result.NewRaisesOccurred && currentIndex != 0)
                {
                    _context.IsWaitingForPlayerAction = true;
                    _context.CanAct = true;
                    return;
                }
            }

            if (_context.StateManager.IsBettingRoundComplete())
            {
                await Task.Delay(GameViewModel.STATE_TRANSITION_DELAY_MS);
                _context.StateManager.AdvanceGameState();
            }
        }

        private bool ShouldSkipBot(int index, Player player)
        {
            return index == 0 || player.IsFolded || player.Balance <= 0;
        }

        private bool ShouldCheck(Player player)
        {
            int amountToCall = _context.CurrentBet - player.CurrentBet;
            return amountToCall <= 0 && _context.CurrentBet != 0 && player.CurrentBet == _context.CurrentBet;
        }

        private async Task<BotActionResult> ProcessBotAction(int index, Player player, ActionType actionType)
        {
            IPlayerAction action;

            switch (actionType)
            {
                case ActionType.Fold:
                    if (index == GameViewModel.LAST_BOT_INDEX && _context.PlayerBalance == 0)
                        action = new CallAction();
                    else
                        action = new FoldAction();
                    break;

                case ActionType.Call:
                    if (index == GameViewModel.FIRST_BOT_INDEX && _context.PlayerBalance == 0)
                        action = new FoldAction();
                    else
                        action = new CallAction();
                    break;

                case ActionType.Raise:
                    if (_context.BettingManager.RaiseOccurredInRound)
                        action = new CallAction();
                    else
                    {
                        int minRaise = Math.Max(_context.CurrentBet + _context.BigBlindAmount, _context.CurrentBet * 2);
                        int raiseAmount = Math.Min(minRaise, player.Balance);
                        action = new RaiseAction(raiseAmount);
                        _context.BettingManager.SetRaiseOccurred(true);
                    }
                    break;

                case ActionType.Check:
                    action = new CheckAction();
                    break;

                default:
                    return new BotActionResult { NewRaisesOccurred = false };
            }

            action.Execute(_context, player);
            _context.UpdateCallCheckButtonText();
            _context.OnPropertyChanged(nameof(GameViewModel.Players));

            if (action is FoldAction)
            {
                await CheckForEarlyWin();
            }

            return new BotActionResult { NewRaisesOccurred = action is RaiseAction };
        }


        private async Task CheckForEarlyWin()
        {
            var activePlayers = _context.GetActivePlayers();
            if (activePlayers.Count == 1)
            {
                var winner = activePlayers.First();
                winner.Balance += _context.PotSize;
                if (winner == _context.GetPlayer(0))
                {
                    _context.PlayerBalance = winner.Balance;
                }
                _context.ShowGameMessage($"{winner.Name} wins ${_context.PotSize} (all others folded)");
                await Task.Delay(GameViewModel.EARLY_WIN_DELAY_MS);
                _context.WinnerManager.CheckGameEndCondition();
            }
        }

        private class BotActionResult
        {
            public bool NewRaisesOccurred { get; set; }
        }
    }
}