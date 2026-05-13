using Blast.Core.Event;

namespace Blast.Core.Logic
{
    public class FireCoordinator
    {
        private readonly GameEventQueue _eventQueue;

        private readonly TargetSelector _targetSelector;

        private LaunchTrayLogic _launchTrayLogic;
        public FireCoordinator(TargetSelector targetSelector, LaunchTrayLogic launchTrayLogic, GameEventQueue eventQueue)
        {
            _targetSelector = targetSelector;
            _launchTrayLogic = launchTrayLogic;
            _eventQueue = eventQueue;
        }


        // TODO[P1]: yaklaþým review edilecek
        public void Tick(float deltaTime)
        {
            int slotIndex = -1;
            foreach (LaunchTraySlotLogic slot in _launchTrayLogic.slotLogics)
            {
                slotIndex += 1;
                var shooter = slot.ShooterLogic;
                if (shooter == null) continue;

                shooter.Tick(deltaTime); // cooldown'u kendi içinde ilerletir

                if (!shooter.IsActive || shooter.IsDepleted) continue;

                if (!shooter.CanFire) continue;

                var targetResult = _targetSelector.FindTarget(shooter.Color);
                if (!targetResult.HasTarget) continue;



                int targetColumn = targetResult.Column;
                int hitRow = targetResult.Row;
                shooter.Fire();
                _eventQueue.Enqueue(new ShooterFiredEvent(shooter.Id, slotIndex, shooter.Color, targetColumn, hitRow, shooter.Ammo));
                //_eventQueue.Enqueue(new CubeHitEvent(targetColumn,hitRow, true));
            }
        }


    }

}





