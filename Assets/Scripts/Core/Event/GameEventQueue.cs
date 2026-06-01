using System.Collections.Generic;


namespace Blast.Core.Event
{
    public class GameEventQueue
    {
        private readonly Queue<IGameEvent> _events = new Queue<IGameEvent>();

        public void Enqueue(IGameEvent gameEvent) => _events.Enqueue(gameEvent);

        public bool TryDequeue(out IGameEvent gameEvent)
        {
            if (_events.Count > 0)
            {
                gameEvent = _events.Dequeue();
                return true;
            }
            gameEvent = null;
            return false;
        }

        public void Clear() => _events.Clear();
        public int Count => _events.Count;
    }
}