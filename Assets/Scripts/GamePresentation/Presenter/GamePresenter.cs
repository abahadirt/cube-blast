using Blast.Core.Event;
using Blast.Core.Logic;
using Blast.GamePresentation.Contract;
using Blast.Logging;
using System;

namespace Blast.GamePresentation.Presenter
{
    public class GamePresenter
    {
        
        public event Action LevelCompleted;
        public event Action LevelFailed;

        private readonly GameplayLogic _gameplayLogic;
        private readonly GameEventQueue _eventQueue;

        private readonly BoardPresenter _boardPresenter;
        private readonly ShooterReservePresenter _reservePresenter;
        private readonly LaunchTrayPresenter _launchTrayPresenter;
        
        private readonly IProjectileLauncher _projectileLauncher;

        public GamePresenter(
            GameplayLogic gameplayLogic, BoardPresenter boardPresenter, ShooterReservePresenter reservePresenter, LaunchTrayPresenter launchTrayPresenter,
            GameEventQueue eventQueue,
            IProjectileLauncher projectileLauncher)
        {
            _gameplayLogic = gameplayLogic;
            _boardPresenter = boardPresenter;
            _reservePresenter = reservePresenter;
            _launchTrayPresenter = launchTrayPresenter;
            _eventQueue = eventQueue;
            _projectileLauncher = projectileLauncher;
        }

        public void Initialize()
        {
            _boardPresenter.Initialize();
            _reservePresenter.Initialize();
        }

        public void Tick(float deltaTime)
        {
            _gameplayLogic.Tick(deltaTime);


            ProcessEvents();
        }

        public void TrySendShooter(int columnIndex)
        {
            _gameplayLogic.SendShooterToLaunchTray(columnIndex);
           
        }

        private void ProcessEvents()
        {
            while (_eventQueue.TryDequeue(out IGameEvent gameEvent))
            {
                switch (gameEvent)
                {
                    case ShooterSentEvent e:
                        _reservePresenter.ReleaseShooter(e.SourceColumnIndex);
                        _launchTrayPresenter.ReceiveShooter(e.ShooterId, e.TargetSlotIndex, e.ArrivalDuration);
                        Log.Info(nameof(GamePresenter), $"ShooterSentEvent işlendi: ShooterId={e.ShooterId}, TargetSlotIndex={e.TargetSlotIndex}, ArrivalDuration={e.ArrivalDuration}");
                        break;

                    case ShootersMergedEvent e:
                        _launchTrayPresenter.MergeShooters(e.SurvivorShooterId, e.ConsumedShooterIds, e.TotalAmmo);
                        Log.Info(nameof(GamePresenter), $"ShootersMergedEvent işlendi: SurvivorShooterId={e.SurvivorShooterId}, ConsumedShooterIds=[{string.Join(", ", e.ConsumedShooterIds)}], TotalAmmo={e.TotalAmmo}");
                        break;
                    case ShooterFiredEvent e:
                        _launchTrayPresenter.TempResolveShooterFired(e.ShooterId, e.RemainingAmmo);
                        _boardPresenter.EnqueueHit(e.TargetColumn, e.TargetLogicalRow);
                        _projectileLauncher.FireFromTrayToBoard(e.Color, e.SlotIndex,e.TargetColumn,
                            onArrived: () => _boardPresenter.OnProjectileArrived(e.TargetColumn));       
                        break;
                    case LevelCompletedEvent _:
                        Log.Info(nameof(GamePresenter), "LevelCompletedEvent işlendi: Seviye tamamlandı!");
                        LevelCompleted?.Invoke();
                        break;
                    case LevelFailedEvent _:
                        Log.Info(nameof(GamePresenter), "LevelFailedEvent işlendi: Seviye başarısız oldu!");
                        LevelFailed?.Invoke();
                        break;
                    case null:
                        Log.Warn(nameof(GamePresenter), "Kuyruktan NULL bir event çıktı!");
                        break;
                    default:
                        Log.Warn(nameof(GamePresenter), $"Tanımlanmamış bir event yakalandı: {gameEvent.GetType().Name}");
                        break;
                }
            }
        }








    }
}