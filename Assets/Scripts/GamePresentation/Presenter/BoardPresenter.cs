using Blast.Core.Data;
using Blast.Core.Logic;
using Blast.GamePresentation.Contract;
using System.Collections.Generic;
using Blast.Logging;

namespace Blast.GamePresentation.Presenter
{
    public class BoardPresenter
    {
        private readonly BoardLogic _board;
        private readonly IBoardView _view;
        private readonly int _visibleRows;



        /*
          [debuglog 1 ] Hit Column index: 9, hitLogicalRow: 3, newTopDataRow: 13
          [debuglog 2] Hit Column index: 9, hitLogicalRow: 1, newTopDataRow: 11
           mermi ulaşma süresi farkından logicten gelen eventle senkronize olmak için çözüm:
         */
        private readonly Queue<int>[] _pendingHitsPerColumn;

        public BoardPresenter(BoardLogic board, IBoardView view, int visibleRows)
        {
            _board = board;
            _view = view;
            _visibleRows = visibleRows;

            _pendingHitsPerColumn = new Queue<int>[board.Columns];
            for (int i = 0; i < board.Columns; i++)
                _pendingHitsPerColumn[i] = new Queue<int>();
        }

        public void Initialize()
        {
            int cols = _board.Columns;
            var colors = new CubeColor[_visibleRows, cols];
            var active = new bool[_visibleRows, cols];

            for (int col = 0; col < cols; col++)
            {
                int bottom = _board.GetColumnBottom(col);
                int top = _board.GetColumnTop(col);

                for (int v = 0; v < _visibleRows; v++)
                {
                    int dataRow = bottom + v;
                    if (dataRow < top)
                    {
                        colors[v, col] = _board.GetDataAt(dataRow, col).Color;
                        active[v, col] = true;
                    }
                    else
                    {
                        active[v, col] = false;
                    }
                }
            }

            _view.Initialize(cols, _visibleRows, colors, active);
        }

        public void EnqueueHit(int column, int hitLogicalRow)
        {
            _pendingHitsPerColumn[column].Enqueue(hitLogicalRow);
        }

        public void OnProjectileArrived(int column)
        {
            var queue = _pendingHitsPerColumn[column];
            if (queue.Count == 0)
            {
                Log.Warn(nameof(BoardPresenter), $"col {column} projectile arrived, pending hit yok." +
                    "EnqueueHit ile OnProjectileArrived senkron değil.");

                return;
            }

            int hitLogicalRow = queue.Dequeue();
            ResolveHitInternal(column, hitLogicalRow);
        }
        private void ResolveHitInternal(int column, int hitLogicalRow)
        {
            int newTopDataRow = hitLogicalRow + _visibleRows;
            Log.Info(nameof(BoardPresenter), $"ResolveHIT: {column}, hitLogicalRow: {hitLogicalRow}, newTopDataRow: {newTopDataRow}");

            CubeColor? newTopColor = newTopDataRow < _board.GetColumnTop(column)
                ? _board.GetDataAt(newTopDataRow, column).Color
                : (CubeColor?)null;
            _view.RemoveCubeFromBottom(column, newTopColor);
        }
    
    
    
    }

}