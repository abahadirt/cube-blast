using Blast.Core.Event;
using Blast.Core.Logic;
using Blast.GameUnity; //TODO[P0] : test için kullanýlýyor, çýkarýlacak



namespace Blast.GamePresentation.Presenter
{
    public class GamePresenter
    {
        private readonly GameplayLogic _gameplayLogic;
        private readonly GameEventQueue _eventQueue;

        private readonly BoardPresenter _boardPresenter;
        private readonly ShooterReservePresenter _reservePresenter;
        private readonly LaunchTrayPresenter _launchTrayPresenter;
        
        private readonly ProjectileService _projectileService;

        public GamePresenter(
            GameplayLogic gameplayLogic, BoardPresenter boardPresenter, ShooterReservePresenter reservePresenter, LaunchTrayPresenter launchTrayPresenter,
            GameEventQueue eventQueue, 
            ProjectileService projectileService)
        {
            _gameplayLogic = gameplayLogic;
            _boardPresenter = boardPresenter;
            _reservePresenter = reservePresenter;
            _launchTrayPresenter = launchTrayPresenter;
            _eventQueue = eventQueue;
            _projectileService = projectileService;
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
                        UnityEngine.Debug.Log($"ShooterSentEvent iþlendi: ShooterId={e.ShooterId}, TargetSlotIndex={e.TargetSlotIndex}, ArrivalDuration={e.ArrivalDuration}");
                        break;

                    case ShootersMergedEvent e:
                        _launchTrayPresenter.MergeShooters(e.SurvivorShooterId, e.ConsumedShooterIds, e.TotalAmmo);
                        UnityEngine.Debug.Log($"ShootersMergedEvent iþlendi: SurvivorShooterId={e.SurvivorShooterId}, ConsumedShooterIds=[{string.Join(", ", e.ConsumedShooterIds)}], TotalAmmo={e.TotalAmmo}");
                        break;
                    case ShooterFiredEvent e:
                        _launchTrayPresenter.TempResolveShooterFired(e.ShooterId, e.RemainingAmmo);
                        _boardPresenter.EnqueueHit(e.TargetColumn, e.TargetLogicalRow);
                        _projectileService.FireFromTrayToBoard(e.Color, e.SlotIndex,e.TargetColumn,
                            onArrived: () => _boardPresenter.OnProjectileArrived(e.TargetColumn));
                        // _launchTrayPresenter.PlayFireAnimation(...)
                        //UnityEngine.Debug.Log($"ShooterFiredEvent iþlendi: ShooterId={e.ShooterId}, Color={e.Color},TargetColumnIndex={e.TargetColumn}");
                        break;
                    /*
                    case CubeHitEvent e:
                        //_boardPresenter.ResolveHit(e.Column, e.DestroyedLogicalRow);
                        // _boardPresenter.PlayCubeDestroyAnimation(e.Column, e.HitRow);
                        //UnityEngine.Debug.Log($"CubeHitEvent iþlendi: Column={e.Column}, DestroyedLogicalRow={e.DestroyedLogicalRow}, IsDestroyed={e.IsDestroyed}");
                        break;
                    */
                    case null:
                        UnityEngine.Debug.LogWarning("Kuyruktan NULL bir event çýktý! Logic katmanýnda bir hata olabilir.");
                        break;

                    default:
                        UnityEngine.Debug.LogWarning($"Tanýmlanmamýþ bir event yakalandý: {gameEvent.GetType().Name}");
                        break;
                }
            }
        }








    }
}