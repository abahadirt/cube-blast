using System;
using Blast.GamePresentation.Presenter;
using Blast.GameUnity.Input;
using Blast.GameUnity.Level;
using Blast.GameUnity.UI;
using UnityEngine.SceneManagement;

namespace Blast.GameUnity.Boot
{
    /// <summary>
    /// Manages the game flow and level transitions.
    /// The composition root ONLY builds the graph; flow logic (tick timing, win/loss) belongs here.
    /// It is scene-scoped due to scene reloads, NOT a persistent manager/singleton.
    /// </summary>
    public class GameFlowController : IDisposable
    {
        private readonly GamePresenter _presenter;
        private readonly LevelCatalog _catalog;
        private readonly LevelEndView _endView;
        private readonly InputHandler _inputHandler;
        private readonly int _levelIndex;

        private bool _resolved;

        public GameFlowController(GamePresenter presenter, LevelCatalog catalog,
            LevelEndView endView, InputHandler inputHandler, int levelIndex)
        {
            _presenter = presenter;
            _catalog = catalog;
            _endView = endView;
            _inputHandler = inputHandler;
            _levelIndex = levelIndex;

            _presenter.LevelCompleted += OnLevelCompleted;
            _presenter.LevelFailed += OnLevelFailed;
        }

        public void Tick(float deltaTime)
        {
            if (_resolved) return;
            _presenter.Tick(deltaTime);
        }

        private void OnLevelCompleted()
        {
            _resolved = true;
            _inputHandler.enabled = false;             // block taps while overlay is active
            _endView.ShowWin(AdvanceToNextLevel);
        }

        private void OnLevelFailed()
        {
            _resolved = true;
            _inputHandler.enabled = false;
            _endView.ShowLose(ReloadScene);       // keep index -> replay level
        }

        private void AdvanceToNextLevel()
        {
            int next = _levelIndex + 1;
            LevelProgress.CurrentIndex = _catalog.IsValidIndex(next) ? next : 0; // wrap around at the end
            ReloadScene();
        }

        private void ReloadScene() =>
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        public void Dispose()
        {
            _presenter.LevelCompleted -= OnLevelCompleted;
            _presenter.LevelFailed -= OnLevelFailed;
        }
    }
}