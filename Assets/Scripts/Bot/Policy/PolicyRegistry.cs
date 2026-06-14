using System;
using System.Collections.Generic;

namespace Blast.Bot.Policy
{
    /// <summary>
    /// Registry for available bot policy implementations.
    /// </summary>
    public sealed class PolicyRegistry
    {
        public IReadOnlyList<string> Names { get; } = new[] { "stochastic" };

        public IBotPolicy Create(string name)
        {
            switch (name.ToLowerInvariant())
            {
                case "stochastic": return new StochasticPolicy();
                default:
                    throw new ArgumentException($"Unknown policy '{name}' ({string.Join("|", Names)}).");
            }
        }
    }
}