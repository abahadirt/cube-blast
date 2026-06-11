using Blast.Core.Event;
using Blast.Core.Logic;
using Blast.Core.Config;
using Blast.GamePresentation.Presenter;
using Blast.GameUnity.Input;
using Blast.GameUnity.Level;
using Blast.GameUnity.Logging;
using Blast.GameUnity.Registry;
using Blast.GameUnity.UI;
using Blast.GameUnity.View;
using Blast.Level;
using Blast.Logging;
using UnityEngine;

namespace Blast.GameUnity.Boot
{
    /// <summary>
    /// Composition root: builds the object graph for a single level and owns the
    /// per-frame Update loop. It only WIRES dependencies; runtime flow (win/lose,
    /// level transitions) lives in GameFlowController. Scene-scoped: a scene reload
    /// destroys and rebuilds everything created here.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Level Setup")]
        [SerializeField] private LevelCatalog _levelCatalog;
        [SerializeField] private LevelEndView _levelEndView;

        [Header("Views")]
        [SerializeField] private BoardView _boardView;
        [SerializeField] private ShooterReserveView _reserveView;
        [SerializeField] private LaunchTrayView _trayView;
        [SerializeField] private ProjectileLauncher _projectileLauncher;

        [SerializeField] private InputHandler _inputHandler;

        private GamePresenter _gameplayPresenter;
        private GameFlowController _flow;

        private void Awake()
        {
            Log.Configure(new UnityLogger());

            int levelIndex = ResolveStartIndex();
            LevelData levelData = LevelParser.Parse(_levelCatalog.Get(levelIndex).text);

            ShooterViewRegistry registry = new ShooterViewRegistry();
            _reserveView.Construct(registry);
            _trayView.Construct(registry);

            // --- Level data ---
            var rows = levelData.rows;
            var totalRows = levelData.totalRows;
            var columns = levelData.columns;
            var reserveColumns = levelData.reserveColumns;
            var launchTrayCapacity = levelData.launchTrayCapacity;
            var visibleRows = levelData.visibleRows;

            // --- Event ---
            var eventQueue = new GameEventQueue();

            // --- Logic layer ---
            var coreConfig = new CoreConfig(); // use default config values.
            var boardLogic = new BoardLogic(columns, totalRows, rows);
            var trayLogic = new LaunchTrayLogic(launchTrayCapacity, eventQueue, coreConfig);
            var reserveLogic = new ShooterReserveLogic(reserveColumns, coreConfig);
            var targetSelector = new TargetSelector(boardLogic);
            var fireCoord = new FireCoordinator(targetSelector, trayLogic, boardLogic, eventQueue);
            var levelConditionEvaluator = new LevelConditionEvaluator(boardLogic, trayLogic, reserveLogic, eventQueue);
            var gameplayLogic = new GameplayLogic(boardLogic, trayLogic, reserveLogic, targetSelector, fireCoord, eventQueue, levelConditionEvaluator);

            // --- Presenter layer ---
            var boardPresenter = new BoardPresenter(boardLogic, _boardView, visibleRows);
            var reservePresenter = new ShooterReservePresenter(reserveLogic, _reserveView);
            var launchTrayPresenter = new LaunchTrayPresenter(trayLogic, _trayView);
            _gameplayPresenter = new GamePresenter(gameplayLogic, boardPresenter, reservePresenter, launchTrayPresenter, eventQueue, _projectileLauncher);


            _inputHandler.OnColumnTapped += _gameplayPresenter.TrySendShooter;

            _flow = new GameFlowController(_gameplayPresenter, _levelCatalog, _levelEndView, _inputHandler, levelIndex);

            _gameplayPresenter.Initialize();
        }

        private void Update()
        {
            _flow.Tick(Time.deltaTime);
        }

        private int ResolveStartIndex()
        {
            int index = LevelProgress.CurrentIndex;
            if (!_levelCatalog.IsValidIndex(index)) // end of catalog / corrupt index -> wrap to start
            {
                index = 0;
                LevelProgress.CurrentIndex = 0;
            }
            return index;
        }

        private void OnDestroy()
        {
            // Symmetric teardown: pair every subscription made in Awake.
            // Safe-by-scene-scope today, but explicit so it stays correct if anything
            // ever outlives the scene (e.g. a DontDestroyOnLoad service).
            if (_inputHandler != null && _gameplayPresenter != null)
                _inputHandler.OnColumnTapped -= _gameplayPresenter.TrySendShooter;

            _flow?.Dispose();
        }
    }
}