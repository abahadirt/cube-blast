using Blast.Core.Event;

namespace Blast.Bot.Runner
{
    /// <summary>
    /// Receives decisions, gameplay events, and run-end summary data.
    /// </summary>
    public interface IRunObserver
    {
        void OnDecision(int tick, int? column);
        void OnEvent(int tick, IGameEvent gameEvent);
        void OnRunEnd(RunResult result);
    }
}
