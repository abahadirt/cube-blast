using Blast.Bot.Runner;
using Blast.Core.Event;

namespace Blast.Bot.Metrics
{
    /// <summary>
    /// Writes per-event and run-summary metrics for one simulation run.
    /// </summary>
    public sealed class MetricsRecorder : IRunObserver
    {
        private readonly CsvLineWriter _events;
        private readonly CsvLineWriter _runs;

        private readonly int _runId;
        private readonly int _seed;
        private readonly string _levelId;
        private readonly string _policy;
        private readonly string _mode;     // fair - oracle
        private readonly float _dt;
        private readonly string _harnessVersion;
        private readonly int _schemaVersion;

        public MetricsRecorder(CsvLineWriter events, CsvLineWriter runs,
            int runId, int seed, string levelId, string policy, string mode, float dt,
            string harnessVersion, int schemaVersion)
        {
            _events = events; _runs = runs;
            _runId = runId; _seed = seed; _levelId = levelId; _policy = policy; _mode = mode;
            _dt = dt; _harnessVersion = harnessVersion; _schemaVersion = schemaVersion;
        }

        public void OnDecision(int tick, int? column) 
        {
            // Decisions are summarized through run metrics, not written as event rows.
        }

        public void OnEvent(int tick, IGameEvent gameEvent)
        {
            float simTime = tick * _dt;
            _events.Field(_runId).Field(_seed).Field(_levelId).Field(_policy)
                   .Field(tick).Field(simTime);

            switch (gameEvent)
            {
                case ShooterSentEvent e:
                    _events.Field("ShooterSent")
                           .Field(e.ShooterId).Empty().Field(e.SourceColumnIndex).Field(e.TargetSlotIndex)
                           .Empty().Empty().Empty().Empty().Empty().Empty();
                    break;
                case ShooterFiredEvent e:
                    _events.Field("ShooterFired")
                           .Field(e.ShooterId).Field(e.Color.ToString()).Empty().Field(e.SlotIndex)
                           .Field(e.TargetColumn).Field(e.TargetLogicalRow).Field(e.RemainingAmmo)
                           .Empty().Empty().Empty();
                    break;
                case ShootersMergedEvent e:
                    _events.Field("ShootersMerged")
                           .Empty().Empty().Empty().Empty().Empty().Empty().Empty()
                           .Field(e.SurvivorShooterId)
                           .Field(e.ConsumedShooterId1 + ";" + e.ConsumedShooterId2)
                           .Field(e.TotalAmmo);
                    break;
                case LevelCompletedEvent _: WriteTypeOnly("LevelCompleted"); break;
                case LevelFailedEvent _: WriteTypeOnly("LevelFailed"); break;
                default: WriteTypeOnly(gameEvent.GetType().Name); break;
            }
            _events.EndRow();
        }

        public void OnRunEnd(RunResult result)
        {
            _runs.Field(_runId).Field(_seed).Field(_levelId).Field(_policy)
                 .Field(result.Outcome.ToString())
                 .Field(result.Ticks).Field(result.SimTime)
                 .Field(result.TotalTaps).Field(result.TotalShots).Field(result.TotalMerges)
                 .Field(result.TotalCubesCleared).Field(result.TrayFullStalls)
                 .Field(result.ReserveExhausted).Field(result.FinalCubesRemaining)
                 .Field(_dt).Field(_harnessVersion).Field(_schemaVersion)
                 .Field(_mode);
            _runs.EndRow();

            _events.Flush();  // writers buffer during the run; make each completed run durable
            _runs.Flush();
        }

        private void WriteTypeOnly(string eventType)
            => _events.Field(eventType).Empty().Empty().Empty().Empty().Empty().Empty().Empty().Empty().Empty().Empty();
    }
}