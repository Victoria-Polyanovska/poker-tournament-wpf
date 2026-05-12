using TexasHoldemWPF.Models.Entities;
using TexasHoldemWPF.ViewModels;

namespace TexasHoldemWPF.Models.Actions
{
    public interface IPlayerAction
    {
        void Execute(GameViewModel context, Player player);
    }
}
