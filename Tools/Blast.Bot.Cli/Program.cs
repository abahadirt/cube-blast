using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Blast.Bot.Batch;
using Blast.Bot.Metrics;
using Blast.Bot.Observation;
using Blast.Bot.Policy;
using Blast.Bot.Replay;
using Blast.Bot.Runner;
using Blast.Core.Config;
using Blast.Level;

namespace Blast.Bot.Cli
{
    internal static class Program
    {
        private const string HarnessVersion = "1.0";

        private static int Main(string[] args)
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            var reporter = new CliReporter();
            if (args.Length == 0) { reporter.Usage(); return 2; }

            var levels = new LevelRepository();
            var policies = new PolicyRegistry();
            var o = CliOptions.Parse(args);

            var levelIds = ResolveLevels(o, levels);
            var policyNames = ResolvePolicies(o, policies);
            if (levelIds.Count == 0) { reporter.Error("No level specified."); reporter.Usage(); return 2; }

            var sim = new SimConfig { Mode = ParseMode(o.Mode) };
            var core = new CoreConfig();

            bool batch = levelIds.Count > 1 || policyNames.Count > 1 || o.SeedCount > 1 || o.Repeat > 1;
            return batch
                ? RunBatch(levelIds, policyNames, o, sim, core, levels, policies, reporter)
                : RunSingle(levelIds[0], policyNames[0], o, sim, core, levels, policies, reporter);
        }

        private static int RunBatch(List<string> levelIds, List<string> policyNames, CliOptions o,
            SimConfig sim, CoreConfig core, LevelRepository levels, PolicyRegistry policies, CliReporter reporter)
        {
            var cfg = new ExperimentConfig
            {
                Levels = levelIds,
                Policies = policyNames,
                SeedStart = o.SeedStart,
                SeedCount = o.SeedCount,
                Repeat = o.Repeat,
                Sim = sim,
                Core = core,
            };
            var runner = new BatchRunner(levels.Load, policies.Create);
            reporter.BatchStarted(cfg);

            MetricsCsvWriter csv = null;
            try
            {
                RunObserverFactory observerFor = null;
                if (o.CsvDir != null)
                {
                    csv = MetricsCsvWriter.OpenDirectory(o.CsvDir);
                    var session = csv;
                    observerFor = (s, lid, pol, m, dt) => session.CreateRecorder(s, lid, pol, m, dt, HarnessVersion);
                }
                var summary = runner.Run(cfg, observerFor, reporter.Progress);
                reporter.ProgressDone();
                reporter.BatchFinished(summary, o.CsvDir);
            }
            catch (ArgumentException ex) { reporter.ProgressDone(); reporter.Error($"batch aborted: {ex.Message}"); return 2; }
            finally { csv?.Dispose(); }
            return 0;
        }

        private static int RunSingle(string levelId, string policyName, CliOptions o,
            SimConfig sim, CoreConfig core, LevelRepository levels, PolicyRegistry policies, CliReporter reporter)
        {
            string json = levels.LoadJson(levelId);
            if (json == null) { reporter.Error($"Level not found: '{levelId}'."); return 2; }

            var level = LevelParser.Parse(json);
            var policy = policies.Create(policyName);

            var replay = o.RecordPath != null
                ? new ReplayRecorder(BuildManifest(levelId, json, core, sim, policy.Name, o.Mode, o.SeedStart))
                : null;

            MetricsCsvWriter csv = null;
            try
            {
                MetricsRecorder metrics = null;
                if (o.CsvDir != null)
                {
                    csv = MetricsCsvWriter.OpenDirectory(o.CsvDir);
                    metrics = csv.CreateRecorder(seed: o.SeedStart, levelId: levelId, policy: policy.Name,
                                                 mode: o.Mode, dt: sim.Dt, harnessVersion: HarnessVersion); // named args
                }
                IRunObserver observer = (replay != null || metrics != null) ? new CompositeObserver(replay, metrics) : null;

                var r = new SimulationRunner().Run(level, sim, core, policy, o.SeedStart, levelId, observer);
                reporter.RunFinished(r);

                if (replay != null) { ReplayLog.Write(replay.Log, o.RecordPath); reporter.ReplaySaved(o.RecordPath, replay.Log.taps.Count); }
                if (o.CsvDir != null) reporter.CsvSaved(o.CsvDir);
            }
            finally { csv?.Dispose(); }
            return 0;
        }

        // CLI-spec resolution ("all"/csv/positional) stays console-side: it uses the catalogs but is CLI glue.
        private static List<string> ResolveLevels(CliOptions o, LevelRepository levels)
            => o.Levels != null
                ? (o.Levels.Equals("all", StringComparison.OrdinalIgnoreCase) ? new List<string>(levels.EnumerateIds()) : SplitCsv(o.Levels))
                : (o.LevelArg != null ? new List<string> { o.LevelArg } : new List<string>());

        private static List<string> ResolvePolicies(CliOptions o, PolicyRegistry policies)
            => o.Policies != null
                ? (o.Policies.Equals("all", StringComparison.OrdinalIgnoreCase) ? new List<string>(policies.Names) : SplitCsv(o.Policies))
                : new List<string> { o.Policy };

        private static List<string> SplitCsv(string s)
        {
            var list = new List<string>();
            foreach (var part in s.Split(','))
            {
                string t = part.Trim();
                if (t.Length > 0) list.Add(t);
            }
            return list;
        }

        private static ObservationMode ParseMode(string m)
        {
            if (m == null || m.Equals("oracle", StringComparison.OrdinalIgnoreCase))
                return ObservationMode.Oracle;

            if (m.Equals("fair", StringComparison.OrdinalIgnoreCase))
                return ObservationMode.Fair;

            throw new ArgumentException($"Unknown mode '{m}'. Expected fair|oracle.");
        }

        private static ReplayLog BuildManifest(string levelId, string json, CoreConfig core,
            SimConfig sim, string policyName, string mode, int seed)
            => new ReplayLog
            {
                harnessVersion = HarnessVersion,
                levelId = levelId,
                levelHash = Sha256Hex(json),
                coreConfig = new CoreConfigDto { fireCooldown = core.FireCooldown, arrivalDuration = core.ArrivalDuration },
                policy = policyName,
                observationMode = mode,
                seed = seed,
                dt = sim.Dt,
                maxTicks = sim.MaxTicks,
                decisionEveryNTicks = sim.DecisionEveryNTicks,
            };

        private static string Sha256Hex(string text)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}