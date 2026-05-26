using Blast.Core.Event;
using System.Collections.Generic;

namespace Blast.Core.Logic
{
    public class GameplayLogic
    {
        private readonly GameEventQueue _eventQueue;

        private LevelConditionEvaluator _levelConditionEvaluator;

        private BoardLogic _boardLogic;
        private LaunchTrayLogic _launchTrayLogic;
        private ShooterReserveLogic _shooterReserveLogic;
        
        private TargetSelector _targetSelector;
       
        private FireCoordinator _fireCoordinator;

        public GameplayLogic
            (BoardLogic boardLogic,
            LaunchTrayLogic launchTraylogic,
            ShooterReserveLogic shooterReserveLogic,
            TargetSelector targetSelector,
            FireCoordinator fireCoordinator,
            GameEventQueue eventQueue,
            LevelConditionEvaluator levelConditionEvaluator)
        {
            _boardLogic = boardLogic;
            _launchTrayLogic = launchTraylogic;
            _shooterReserveLogic = shooterReserveLogic;
            _targetSelector = targetSelector;
            _fireCoordinator = fireCoordinator;
            _eventQueue = eventQueue;
            _levelConditionEvaluator = levelConditionEvaluator;
        }



        public void SendShooterToLaunchTray(int columnIndex)
        {
            if (columnIndex == -1 || !_launchTrayLogic.HasSpace()) return;

            ShooterLogic shooter = _shooterReserveLogic.GetNextShooter(columnIndex);

            if (shooter == null) return;

            int slotIndex = _launchTrayLogic.AddShooter(shooter);

            float duration = _launchTrayLogic.slotLogics[slotIndex].ArrivalDuration;

            _eventQueue.Enqueue(new ShooterSentEvent(shooter.Id, slotIndex, columnIndex, duration));
        }


        public void Tick(float deltaTime)
        {
            _levelConditionEvaluator.Evaluate();
            // TODO[P1] : mergeResults artik kullanýlmýyor. -> bakilacak.
            List<MergeResult> mergeResults = _launchTrayLogic.Tick(deltaTime);
            _fireCoordinator.Tick(deltaTime);
           
        }


    }
}

