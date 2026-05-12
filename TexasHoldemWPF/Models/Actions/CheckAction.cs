using TexasHoldemWPF.Models.Entities;
using TexasHoldemWPF.ViewModels;

namespace TexasHoldemWPF.Models.Actions
{
    public class CheckAction : BaseAction
    {
        public override void Execute(GameViewModel context, Player player)
        {
            player.LastAction = "Check";
        }
    }
}
