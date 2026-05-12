using TexasHoldemWPF.Models.Entities;
using TexasHoldemWPF.ViewModels;

namespace TexasHoldemWPF.Models.Actions
{
    public abstract class BaseAction : IPlayerAction
    {
        public abstract void Execute(GameViewModel context, Player player);

        protected void UpdatePotAndBalance(GameViewModel context, Player player, int amount)
        {
            player.Balance -= amount;
            context.PotSize += amount;

            if (player == context.GetPlayer(0))
            {
                context.PlayerBalance = player.Balance;
            }
        }
    }
}
