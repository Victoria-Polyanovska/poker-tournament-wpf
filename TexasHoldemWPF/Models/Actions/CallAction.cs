using System;
using TexasHoldemWPF.Models.Entities;
using TexasHoldemWPF.ViewModels;

namespace TexasHoldemWPF.Models.Actions
{
    public class CallAction : IPlayerAction
    {
        public void Execute(GameViewModel context, Player player)
        {
            int amountToCall = context.CurrentBet - player.CurrentBet;
            int actualCallAmount = Math.Min(amountToCall, player.Balance);

            player.Balance -= actualCallAmount;
            context.PotSize += actualCallAmount;
            player.CurrentBet += actualCallAmount;

            if (actualCallAmount == 0 && amountToCall == 0)
            {
                player.LastAction = "Check";
            }
            else
            {
                player.LastAction = player.Balance == 0 ? "All-in" : $"Call ${actualCallAmount}";
            }

            if (player == context.GetPlayer(0))
            {
                context.PlayerBalance = player.Balance;
            }
        }
    }
}
