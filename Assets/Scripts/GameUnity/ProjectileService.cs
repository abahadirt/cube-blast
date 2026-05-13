using System;
using Blast.Core.Data;
using Blast.GameUnity.Pool;
using Blast.GameUnity.View;
using UnityEngine;

namespace Blast.GameUnity
{
    public class ProjectileService : MonoBehaviour
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

        /*
        // TODO[P5] : animler tanýmlanýnca hissiyata göre eklenecek.
        //onaction gerekmeyecek cok buyuk ihtimal.
        public void FireVerticalBolt(
            CubeColor color, int column, int fromRow, int toRow, Action onArrived)
        {
            Vector2 from = _boardView.GetVisualCellPosition(column, fromRow);
            Vector2 to = _boardView.GetVisualCellPosition(column, toRow);
            Launch(color, from, to, onArrived);
        }
        */

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