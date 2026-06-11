using System.IO;
using Blast.Bot.Batch;
using Blast.Bot.Metrics;
using Blast.Bot.Runner;

namespace Blast.Bot.Cli
{
    /// <summary>
    /// Writes bot CLI output to the console.
    /// </summary>
    public sealed class CliReporter
    {
        private bool _progressLineOpen;

        public void RunFinished(RunResult r)
        {
            System.Console.WriteLine(
                $"level={r.LevelId}  policy={r.Policy}  seed={r.Seed}  =>  {r.Outcome}  fp={r.Fingerprint}");
            System.Console.WriteLine(
                $"  ticks={r.Ticks}  simTime={r.SimTime:0.00}s  taps={r.TotalTaps}  sends={r.TotalSends}  " +
                $"shots={r.TotalShots}  merges={r.TotalMerges}  stalls={r.TrayFullStalls}");
            System.Console.WriteLine(
                $"  cleared={r.TotalCubesCleared}  remaining={r.FinalCubesRemaining}  reserveEmpty={r.ReserveExhausted}");
        }

        public void ReplaySaved(string path, int taps)
            => System.Console.WriteLine($"  replay log -> {path}  ({taps} taps)");

        public void CsvSaved(string dir)
            => System.Console.WriteLine(
                $"  csv -> {Path.Combine(dir, MetricsCsvWriter.EventsFileName)} + {MetricsCsvWriter.RunsFileName}");

        public void BatchStarted(ExperimentConfig cfg)
            => System.Console.WriteLine(
                $"batch: {cfg.Levels.Count} level(s) × {cfg.Policies.Count} policy(ies) × " +
                $"{cfg.SeedCount} seed(s) × {cfg.Repeat} repeat = {cfg.TotalRuns} runs  " +
                $"[mode={cfg.ModeName}, dt={cfg.Sim.Dt}]");

        public void Progress(BatchProgress p)
        {
            string line = $"{p.Completed}/{p.Total} ({p.Fraction:0.0%})  {p.RunsPerSecond:0} runs/s  " +
                          $"ETA {p.EtaSeconds:0.0}s  W{p.Wins} L{p.Losses} T{p.Timeouts}";
            if (System.Console.IsOutputRedirected) System.Console.WriteLine("  " + line);
            else { System.Console.Write("\r  " + line.PadRight(72)); _progressLineOpen = true; }
        }

        public void ProgressDone()
        {
            if (_progressLineOpen) { System.Console.WriteLine(); _progressLineOpen = false; }
        }

        public void BatchFinished(BatchSummary s, string csvDir)
        {
            double winRate = s.TotalRuns > 0 ? (double)s.Wins / s.TotalRuns : 0.0;
            System.Console.WriteLine(
                $"  done: {s.TotalRuns} runs in {s.ElapsedSeconds:0.0}s ({s.RunsPerSecond:0} runs/s)  " +
                $"W{s.Wins} L{s.Losses} T{s.Timeouts}  win-rate={winRate:0.000}");
            if (csvDir != null) CsvSaved(csvDir);
            else System.Console.WriteLine("  (no --csv: nothing written — pass --csv DIR for the aggregate-ready CSV)");
        }

        public void Error(string message) => System.Console.Error.WriteLine(message);

        public void Usage() => System.Console.Error.WriteLine(
            "usage:\n" +
            "  single: <level-id|path> [--policy <name>] [--seed N]\n" +
            "          [--mode fair|oracle] [--record path.json] [--csv DIR]\n" +
            "  batch:  [--levels <ids|all>] [--policies <names|all>] [--seeds N] [--repeat R]\n" +
            "          [--seed START] [--mode fair|oracle] [--csv DIR]");
    }
}