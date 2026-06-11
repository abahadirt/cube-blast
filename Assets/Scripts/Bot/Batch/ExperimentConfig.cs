using Blast.Bot.Runner;
using Blast.Core.Config;
using System;
using System.Collections.Generic;

namespace Blast.Bot.Batch
{
    /// <summary>
    /// Configuration for expanding a batch into individual simulation runs.
    /// </summary>
    public sealed class ExperimentConfig
    {
        public IReadOnlyList<string> Levels = Array.Empty<string>();
        public IReadOnlyList<string> Policies = Array.Empty<string>();

        public int SeedStart = 0;
        public int SeedCount = 1;
        public int Repeat = 1; 

        public SimConfig Sim = new SimConfig();
        public CoreConfig Core = new CoreConfig();

        public long TotalRuns => (long)Levels.Count * Policies.Count * SeedCount * Repeat;

        public string ModeName => Sim.Mode.ToString().ToLowerInvariant();

        public void Validate()
        {
            if (Levels == null || Levels.Count == 0) throw new ArgumentException("ExperimentConfig.Levels is empty.");
            if (Policies == null || Policies.Count == 0) throw new ArgumentException("ExperimentConfig.Policies is empty.");
            if (SeedCount < 1) throw new ArgumentException("ExperimentConfig.SeedCount must be >= 1.");
            if (Repeat < 1) throw new ArgumentException("ExperimentConfig.Repeat must be >= 1.");
            if (Sim == null) throw new ArgumentException("ExperimentConfig.Sim is null.");
            if (Core == null) throw new ArgumentException("ExperimentConfig.Core is null.");
        }

        /// <summary>
        /// Expands the batch in deterministic order, advancing the seed range on each repeat.
        /// </summary>
        public IEnumerable<RunSpec> Enumerate()
        {
            int index = 0;
            for (int li = 0; li < Levels.Count; li++)
                for (int pi = 0; pi < Policies.Count; pi++)
                    for (int ri = 0; ri < Repeat; ri++)
                        for (int si = 0; si < SeedCount; si++)
                        {
                            int seed = SeedStart + ri * SeedCount + si;
                            yield return new RunSpec(index++, li, Levels[li], pi, Policies[pi], ri, si, seed);
                        }
        }
    }

    /// <summary>
    /// Identifies one simulation run within a batch.
    /// </summary>
    public readonly struct RunSpec
    {
        public readonly int Index;
        public readonly int LevelIndex;
        public readonly string LevelId;
        public readonly int PolicyIndex;
        public readonly string PolicyName;
        public readonly int RepeatIndex;
        public readonly int SeedIndex;
        public readonly int Seed;

        public RunSpec(int index, int levelIndex, string levelId, int policyIndex, string policyName,
                       int repeatIndex, int seedIndex, int seed)
        {
            Index = index;
            LevelIndex = levelIndex;
            LevelId = levelId;
            PolicyIndex = policyIndex;
            PolicyName = policyName;
            RepeatIndex = repeatIndex;
            SeedIndex = seedIndex;
            Seed = seed;
        }
    }
}
