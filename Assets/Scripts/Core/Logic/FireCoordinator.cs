using Blast.Core.Event;

namespace Blast.Core.Logic
{
    public class FireCoordinator
    {
        private readonly GameEventQueue _eventQueue;

        private readonly TargetSelector _targetSelector;

        private LaunchTrayLogic _launchTrayLogic;
        private readonly BoardLogic _boardLogic;
        public FireCoordinator(TargetSelector targetSelector, LaunchTrayLogic launchTrayLogic, BoardLogic boardLogic, GameEventQueue eventQueue)
        {
            _targetSelector = targetSelector;
            _launchTrayLogic = launchTrayLogic;
            _boardLogic = boardLogic;
            _eventQueue = eventQueue;
        }


        public void Tick(float deltaTime)
        {
            int slotIndex = -1;
            foreach (LaunchTraySlotLogic slot in _launchTrayLogic.slotLogics)
            {
                slotIndex += 1;
                var shooter = slot.ShooterLogic;
                if (shooter == null) continue;

                shooter.Tick(deltaTime);

                if (!shooter.IsActive || shooter.IsDepleted) continue;

                if (!shooter.CanFire) continue;

                var targetResult = _targetSelector.SelectTarget(shooter.Color);
                if (!targetResult.HasTarget) continue;

                int targetColumn = targetResult.Column;
                int hitRow = targetResult.Row;

                _boardLogic.LogicalHit(targetColumn); // pre emptive hit
                shooter.Fire();

                _eventQueue.Enqueue(new ShooterFiredEvent(shooter.Id, slotIndex, shooter.Color, targetColumn, hitRow, shooter.Ammo));
            }
        }


    }

}





