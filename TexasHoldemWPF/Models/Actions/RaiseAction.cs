using System;
using TexasHoldemWPF.Models.Entities;
using TexasHoldemWPF.ViewModels;

namespace TexasHoldemWPF.Models.Actions
{
    public class RaiseAction : BaseAction
    {
        private readonly int _amount;

        public RaiseAction(int amount)
        {
            _amount = amount;
        }

        public override void Execute(GameViewModel context, Player player)
        {
            int actualRaiseAmount = Math.Min(_amount, player.Balance);

            player.CurrentBet += actualRaiseAmount;
            
            if (player.CurrentBet > context.CurrentBet)
            {
                context.CurrentBet = player.CurrentBet;
            }

            UpdatePotAndBalance(context, player, actualRaiseAmount);
            player.LastAction = player.Balance == 0 ? "All-in" : $"Raise ${actualRaiseAmount}";
        }
    }
}
