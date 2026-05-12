using System.Collections.Generic;
using TexasHoldemWPF.ViewModels;

namespace TexasHoldemWPF.Models.Strategies
{
    public interface IRaiseOptionsStrategy
    {
        (List<int> Options, string Text) GetOptions(GameViewModel context);
    }

    public class PreFlopRaiseStrategy : IRaiseOptionsStrategy
    {
        public (List<int> Options, string Text) GetOptions(GameViewModel context)
        {
            var options = new List<int> {
                (int)(context.BigBlindAmount * 2.5),
                context.BigBlindAmount * 4,
                context.BigBlindAmount * 6,
                context.PlayerBalance
            };
            return (options, "2.5BB|4BB|6BB|ALL IN");
        }
    }

    public class PostFlopRaiseStrategy : IRaiseOptionsStrategy
    {
        public (List<int> Options, string Text) GetOptions(GameViewModel context)
        {
            if (context.CurrentBet == 0)
            {
                var options = new List<int> {
                    (int)(context.PotSize * 0.5),
                    (int)(context.PotSize * 0.666),
                    (int)(context.PotSize * 0.75),
                    context.PlayerBalance
                };
                return (options, "1/2 POT|2/3 POT|3/4 POT|ALL IN");
            }

            return (new List<int> {
                context.CurrentBet * 2,
                context.CurrentBet * 3,
                context.CurrentBet * 4,
                context.PlayerBalance
            }, "X2|X3|X4|ALL IN");
        }
    }
}