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
        private const string RaiseOptionsText = "2.5BB|4BB|6BB|ALL IN";

        public (List<int> Options, string Text) GetOptions(GameViewModel context)
        {
            var options = new List<int>
            {
                (int)(context.BigBlindAmount * 2.5),
                context.BigBlindAmount * 4,
                context.BigBlindAmount * 6,
                context.PlayerBalance
            };

            return (options, RaiseOptionsText);
        }
    }

    public class PostFlopRaiseStrategy : IRaiseOptionsStrategy
    {
        private const string PotRaiseOptionsText = "1/2 POT|2/3 POT|3/4 POT|ALL IN";
        private const string BetMultiplierOptionsText = "X2|X3|X4|ALL IN";

        public (List<int> Options, string Text) GetOptions(GameViewModel context)
        {
            if (context.CurrentBet == 0)
            {
                return GetPotBasedOptions(context);
            }

            return GetBetMultiplierOptions(context);
        }

        private static (List<int> Options, string Text) GetPotBasedOptions(GameViewModel context)
        {
            var options = new List<int>
            {
                (int)(context.PotSize * 0.5),
                (int)(context.PotSize * 0.666),
                (int)(context.PotSize * 0.75),
                context.PlayerBalance
            };

            return (options, PotRaiseOptionsText);
        }

        private static (List<int> Options, string Text) GetBetMultiplierOptions(GameViewModel context)
        {
            var options = new List<int>
            {
                context.CurrentBet * 2,
                context.CurrentBet * 3,
                context.CurrentBet * 4,
                context.PlayerBalance
            };

            return (options, BetMultiplierOptionsText);
        }
    }
}