using System;
using Blast.Core.Data;
using Blast.GameUnity.Pool;
using UnityEngine;
using Blast.GamePresentation.Contract;
namespace Blast.GameUnity.View
{
    public class ProjectileLauncher : MonoBehaviour, IProjectileLauncher
    {
        [SerializeField] private ProjectileViewPool _pool;
        [SerializeField] private BoardView _boardView;
        [SerializeField] private LaunchTrayView _trayView;


        public void FireFromTrayToBoard(CubeColor color, int sourceSlot, int targetColumn, Action onArrived)
        {
            Vector2 from = _trayView.GetSlotPosition(sourceSlot);
            Vector2 to = _boardView.GetBottomCubePosition(targetColumn);
            Launch(color, from, to, onArrived);
        }

        private void Launch(CubeColor color, Vector2 from, Vector2 to, Action onArrived)
        {
            var view = _pool.Get();
            if (view == null) { onArrived?.Invoke(); return; }

            view.Init(color, from, to, () =>
            {
                onArrived?.Invoke();
                _pool.Release(view);
            });
        }
    }
}