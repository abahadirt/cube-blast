using System;
using Blast.Bot.Observation;

namespace Blast.Bot.Policy
{
    /// <summary>
    /// Random baseline policy that picks a non-empty reserve column when the tray has space.
    /// </summary>
    public sealed class StochasticPolicy : IBotPolicy
    {
        public string Name => "Stochastic";

        public int? Decide(GameObservation obs, Random rng)
        {
            if (!obs.HasTraySpace) return null;

            int count = 0;
            for (int c = 0; c < obs.ReserveColCount; c++)
                if (obs.ReserveColHasShooter(c)) count++;
            if (count == 0) return null;

            int pick = rng.Next(count);
            for (int c = 0; c < obs.ReserveColCount; c++)
                if (obs.ReserveColHasShooter(c) && pick-- == 0)
                    return c;

            return null;
        }
    }
}
