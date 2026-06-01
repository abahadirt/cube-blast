using Blast.Core.Event;
using Blast.Logging;

namespace Blast.Core.Logic
{
    public class LevelConditionEvaluator
    {
        private readonly BoardLogic _board;
        private readonly LaunchTrayLogic _tray;
        private readonly ShooterReserveLogic _reserve;
        private readonly GameEventQueue _eventQueue;

        private bool _isResolved = false;

        public LevelConditionEvaluator(
            BoardLogic board,
            LaunchTrayLogic tray,
            ShooterReserveLogic reserve,
            GameEventQueue eventQueue)
        {
            _board = board;
            _tray = tray;
            _reserve = reserve;
            _eventQueue = eventQueue;
        }


        // Polling is used instead of events/dirty flags for simplicity. 
        // Cost is negligible: O(tray * columns).
        public void Evaluate()
        {
            if (_isResolved) return;

            // Win condition takes priority over lose condition.
            if (_board.IsCleared())
            {
                _isResolved = true;
                Log.Info(nameof(LevelConditionEvaluator), "Game Won — board cleared.");
                _eventQueue.Enqueue(new LevelCompletedEvent());
                return;
            }

            if (CanMakeProgress()) return;

            _isResolved = true;
            Log.Info(nameof(LevelConditionEvaluator), "Game Lost — no progress possible.");
            _eventQueue.Enqueue(new LevelFailedEvent());
        }

        // Checks for deadlocks: no one can fire and no new shooters can enter.
        private bool CanMakeProgress()
        {
            for (int i = 0; i < _tray.slotLogics.Length; i++)
            {
                var slot = _tray.slotLogics[i];
                if (slot.IsAvailable) continue;

                // Wait for moving shooters to arrive.
                if (!slot.HasArrived) return true;

                var shooter = slot.ShooterLogic;
                if (shooter.IsDepleted) continue;

                if (_board.HasAnyValidTarget(shooter.Color)) return true;
            }

            // No active shooters can fire. Check if a new one can enter.
            if (_tray.HasSpace() && !_reserve.IsEmpty()) return true;

            return false;
        }
    }
}