using Blast.Bot.Runner;
using Blast.Core.Event;

namespace Blast.Bot.Replay
{
    /// <summary>
    /// Records tap decisions and final run data into a replay log.
    /// </summary>
    public sealed class ReplayRecorder : IRunObserver
    {
        public ReplayLog Log { get; }

        public ReplayRecorder(ReplayLog manifest)
        {
            Log = manifest;
        }

        public void OnDecision(int tick, int? column)
        {
            if (column.HasValue)
                Log.taps.Add(new TapEntry(tick, column.Value));
        }

        public void OnEvent(int tick, IGameEvent gameEvent)
        {
            // Replay only needs the tap schedule; events will be regenerated during playback.
        }

        public void OnRunEnd(RunResult result)
        {
            Log.result = result.Outcome.ToString();
            Log.ticksToResolve = result.Ticks;
            Log.eventFingerprint = result.Fingerprint;
        }
    }
}