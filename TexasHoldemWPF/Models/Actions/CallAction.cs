using System;
using TexasHoldemWPF.Models.Entities;
using TexasHoldemWPF.ViewModels;

namespace TexasHoldemWPF.Models.Actions
{
    public class CallAction : BaseAction
    {
        public override void Execute(GameViewModel context, Player player)
        {
            int callAmount = Math.Min(context.CurrentBet - player.CurrentBet, player.Balance);

            if (callAmount <= 0)
            {
                player.LastAction = "Check";
                return;
            }

            player.CurrentBet += callAmount;
            UpdatePotAndBalance(context, player, callAmount);
            player.LastAction = $"Call ${callAmount}";
        }
    }
}
