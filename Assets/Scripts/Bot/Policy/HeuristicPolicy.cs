using System;
using Blast.Bot.Observation;
using Blast.Core.Data;

namespace Blast.Bot.Policy
{
    /// <summary>
    /// Search-less "smart" policy
    /// </summary>
    public sealed class HeuristicPolicy : IBotPolicy
    {
        // Scoring weights — the policy's value system, made explicit so it's legible and tunable.
        private const int CompleteMergeScore = 1000; // tapping completes a merge triple
        private const int BuildTowardMergeScore = 200; // builds toward a merge
        private const int HasTargetBonus = 10;  // can clear something now (taller preferred, added on top)
        private const int NoTargetPenalty = 50;  // no board target -> would idle in the tray

        public string Name => "Heuristic";

        public int? Decide(GameObservation obs, Random rng)
        {
            return null;
        }
    }
}
