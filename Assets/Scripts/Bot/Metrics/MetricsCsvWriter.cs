using System;
using System.IO;
using System.Text;

namespace Blast.Bot.Metrics
{
    /// <summary>
    /// Writes events.csv and runs.csv for one metrics session.
    /// </summary>
    public sealed class MetricsCsvWriter : IDisposable
    {
        public const int Version = 1;
        public const string EventsFileName = "events.csv";
        public const string RunsFileName = "runs.csv";

        public const string EventHeader =
            "run_id,seed,level_id,policy,tick,sim_time,event_type," +
            "shooter_id,color,source_column,slot_index,target_column,target_row,remaining_ammo," +
            "merge_survivor_id,merge_consumed_ids,merge_total_ammo";

        public const string RunHeader =
            "run_id,seed,level_id,policy,result,ticks_to_resolve,sim_time_to_resolve," +
            "total_taps,total_shots,total_merges,total_cubes_cleared,tray_full_stalls," +
            "reserve_exhausted,final_cubes_remaining,dt,harness_version,schema_version,observation_mode";

        private readonly CsvLineWriter _events;
        private readonly CsvLineWriter _runs;
        private int _nextRunId;

        public MetricsCsvWriter(TextWriter eventsWriter, TextWriter runsWriter)
        {
            _events = new CsvLineWriter(eventsWriter);
            _runs = new CsvLineWriter(runsWriter);
            _events.RawLine(EventHeader);
            _runs.RawLine(RunHeader);
        }


        public static MetricsCsvWriter OpenDirectory(string directory)
        {
            Directory.CreateDirectory(directory);
            return new MetricsCsvWriter(
                NewFile(Path.Combine(directory, EventsFileName)),
                NewFile(Path.Combine(directory, RunsFileName)));
        }

        public MetricsRecorder CreateRecorder(int seed, string levelId, string policy, string mode, float dt, string harnessVersion)
            => new MetricsRecorder(_events, _runs, _nextRunId++, seed, levelId, policy, mode, dt, harnessVersion, Version);

        public void Dispose()
        {
            _events.Dispose();
            _runs.Dispose();
        }

        private static StreamWriter NewFile(string path)
        {
            var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            return new StreamWriter(stream, new UTF8Encoding(false)); // no BOM
        }
    }
}