using Blast.Core.Event;

namespace Blast.Bot.Runner
{
    /// <summary>
    /// Forwards simulation callbacks to multiple observers.
    /// </summary>
    public sealed class CompositeObserver : IRunObserver
    {
        private readonly IRunObserver[] _observers;

        public CompositeObserver(params IRunObserver[] observers)
            => _observers = observers ?? new IRunObserver[0];

        public void OnDecision(int tick, int? column)
        {
            foreach (var o in _observers) o?.OnDecision(tick, column);
        }

        public void OnEvent(int tick, IGameEvent gameEvent)
        {
            foreach (var o in _observers) o?.OnEvent(tick, gameEvent);
        }

        public void OnRunEnd(RunResult result)
        {
            foreach (var o in _observers) o?.OnRunEnd(result);
        }
    }
}