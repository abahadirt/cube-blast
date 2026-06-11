using System;
using Blast.Bot.Observation;

namespace Blast.Bot.Policy
{
    public sealed class GreedyPolicy : IBotPolicy
    {
        public string Name => "Greedy";

        public int? Decide(GameObservation obs, Random rng)
        {
            return null;
        }
    }
}
