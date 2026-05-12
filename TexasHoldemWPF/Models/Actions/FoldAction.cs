using TexasHoldemWPF.Models.Entities;
using TexasHoldemWPF.ViewModels;

namespace TexasHoldemWPF.Models.Actions
{
    public class FoldAction : IPlayerAction
    {
        public void Execute(GameViewModel context, Player player)
        {
            player.IsFolded = true;
            player.LastAction = "Fold";
            
            if (player is BotPlayer bot)
            {
                context.ShowGameMessage(context.GameMessage + $"\n{bot.Name} folded.");
            }
        }
    }
}
