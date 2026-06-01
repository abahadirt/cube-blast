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
        

        // sadelik ve mimari uyumu icin, (win lose condition check), event veya dirtflag yerine polling tercih edildi
        // maliyeti onemsiz duzeyde O(tray*columns)
        public void Evaluate()
        {
            if (_isResolved) return;

            // Win, Lose'dan önce: son atış tahtayı bitiriyorsa kazanç bastırır.
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

        // deadlock durumunu kontrol eder: hiç kimse ateş edemiyor ve yeni biri giremiyor mu?
        private bool CanMakeProgress()
        {
            for (int i = 0; i < _tray.slotLogics.Length; i++)
            {
                var slot = _tray.slotLogics[i];
                if (slot.IsAvailable) continue;

                // hasArrived olmus mu?
                if (!slot.HasArrived) return true;

                var shooter = slot.ShooterLogic;
                if (shooter.IsDepleted) continue;

                if (_board.HasAnyValidTarget(shooter.Color)) return true;
            }

            // Tray'deki hiç kimse ateş edemiyor. Yeni biri girebilir mi?
            if (_tray.HasSpace() && !_reserve.IsEmpty()) return true;

            return false;
        }
    }
}