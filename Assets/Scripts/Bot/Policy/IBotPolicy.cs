using System;
using Blast.Bot.Observation;

namespace Blast.Bot.Policy
{
    /// <summary>
    /// Returns a reserve column to tap, or null to wait.
    /// </summary>
    public interface IBotPolicy
    {
        string Name { get; }
        int? Decide(GameObservation obs, Random rng);
    }
}
