using System;
using System.Collections.Generic;
using System.Diagnostics;
using Blast.Bot.Policy;
using Blast.Bot.Runner;
using Blast.Level;

namespace Blast.Bot.Batch
{
    public delegate IRunObserver RunObserverFactory(int seed, string levelId, string policyName, string mode, float dt);

    /// <summary>
    /// Runs expanded batch simulations and reports aggregate progress.
    /// </summary>
    public sealed class BatchRunner
    {
        private readonly Func<string, LevelData> _loadLevel;
        private readonly Func<string, IBotPolicy> _makePolicy;
        private readonly SimulationRunner _runner = new SimulationRunner();

        public BatchRunner(Func<string, LevelData> loadLevel, Func<string, IBotPolicy> makePolicy)
        {
            _loadLevel = loadLevel ?? throw new ArgumentNullException(nameof(loadLevel));
            _makePolicy = makePolicy ?? throw new ArgumentNullException(nameof(makePolicy));
        }

        public BatchSummary Run(
            ExperimentConfig cfg,
            RunObserverFactory observerFor = null,
            Action<BatchProgress> onProgress = null,
            double progressIntervalMs = 250.0)
        {
            if (cfg == null) throw new ArgumentNullException(nameof(cfg));
            cfg.Validate();

            var levels = new Dictionary<string, LevelData>();
            foreach (string id in cfg.Levels)
            {
                if (levels.ContainsKey(id)) continue;
                LevelData data = _loadLevel(id)
                    ?? throw new ArgumentException($"Level could not be loaded: '{id}'.");
                levels[id] = data;
            }

            string mode = cfg.ModeName;
            long total = cfg.TotalRuns;
            long completed = 0, wins = 0, losses = 0, timeouts = 0;

            var clock = Stopwatch.StartNew();
            double lastReportMs = -progressIntervalMs;

            foreach (RunSpec spec in cfg.Enumerate())
            {
                LevelData level = levels[spec.LevelId];
                IBotPolicy policy = _makePolicy(spec.PolicyName);
                IRunObserver observer = observerFor?.Invoke(spec.Seed, spec.LevelId, policy.Name, mode, cfg.Sim.Dt);

                RunResult r = _runner.Run(level, cfg.Sim, cfg.Core, policy, spec.Seed, spec.LevelId, observer);

                completed++;
                switch (r.Outcome)
                {
                    case RunOutcome.Win: wins++; break;
                    case RunOutcome.Lose: losses++; break;
                    default: timeouts++; break;
                }

                if (onProgress != null)
                {
                    double nowMs = clock.Elapsed.TotalMilliseconds;
                    if (completed == total || nowMs - lastReportMs >= progressIntervalMs)
                    {
                        lastReportMs = nowMs;
                        onProgress(new BatchProgress(completed, total, clock.Elapsed.TotalSeconds,
                                                     wins, losses, timeouts));
                    }
                }
            }

            clock.Stop();
            return new BatchSummary(total, wins, losses, timeouts, clock.Elapsed.TotalSeconds);
        }
    }

    /// <summary>
    /// Current progress snapshot for a running batch.
    /// </summary>
    public readonly struct BatchProgress
    {
        public readonly long Completed;
        public readonly long Total;
        public readonly double ElapsedSeconds;
        public readonly long Wins;
        public readonly long Losses;
        public readonly long Timeouts;

        public BatchProgress(long completed, long total, double elapsedSeconds, long wins, long losses, long timeouts)
        {
            Completed = completed;
            Total = total;
            ElapsedSeconds = elapsedSeconds;
            Wins = wins;
            Losses = losses;
            Timeouts = timeouts;
        }

        public double Fraction => Total > 0 ? (double)Completed / Total : 0.0;
        public double RunsPerSecond => ElapsedSeconds > 0 ? Completed / ElapsedSeconds : 0.0;
        public double EtaSeconds
        {
            get
            {
                double rps = RunsPerSecond;
                return rps > 0 ? (Total - Completed) / rps : 0.0;
            }
        }
    }

    /// <summary>
    /// Final summary for a completed batch.
    /// </summary>
    public readonly struct BatchSummary
    {
        public readonly long TotalRuns;
        public readonly long Wins;
        public readonly long Losses;
        public readonly long Timeouts;
        public readonly double ElapsedSeconds;

        public BatchSummary(long totalRuns, long wins, long losses, long timeouts, double elapsedSeconds)
        {
            TotalRuns = totalRuns;
            Wins = wins;
            Losses = losses;
            Timeouts = timeouts;
            ElapsedSeconds = elapsedSeconds;
        }

        public double RunsPerSecond => ElapsedSeconds > 0 ? TotalRuns / ElapsedSeconds : 0.0;
    }
}
