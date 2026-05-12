using System;
using TexasHoldemWPF.Models.Entities;
using TexasHoldemWPF.ViewModels;

namespace TexasHoldemWPF.Models.Actions
{
    public class RaiseAction : IPlayerAction
    {
        private readonly int _amount;

        public RaiseAction(int amount)
        {
            _amount = amount;
        }

        public void Execute(GameViewModel context, Player player)
        {
            int actualRaiseAmount = Math.Min(_amount, player.Balance);

            player.Balance -= actualRaiseAmount;
            context.PotSize += actualRaiseAmount;
            
            // In Texas Hold'em, a raise is usually relative to the total bet in the round
            // or absolute. The existing code handles it as an increment to the current player's bet
            // but also updates the context.CurrentBet.
            
            player.CurrentBet += actualRaiseAmount;
            
            if (player.CurrentBet > context.CurrentBet)
            {
                context.CurrentBet = player.CurrentBet;
            }

            player.LastAction = player.Balance == 0 ? "All-in" : $"Raise ${actualRaiseAmount}";
            
            if (player == context.GetPlayer(0))
            {
                context.PlayerBalance = player.Balance;
            }
        }
    }
}
