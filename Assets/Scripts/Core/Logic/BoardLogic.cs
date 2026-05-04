using Blast.Core.Data;
using System;

namespace Blast.Core.Logic
{
    public class BoardLogic
    {
        private TargetSelector _targetSelector;

        public int Columns { get; private set; }
        public int TotalRows { get; private set; }

        private CubeData[,] _gridData;
        private int[] _columnBottom;
        private int[] _columnTop;

        //public void Initialize(int columns, int totalRows, GridRow[] rows, TargetSelector targetSelector)
        public BoardLogic(int columns, int totalRows, GridRow[] rows)
        {
            Columns = columns;
            TotalRows = totalRows;

            _gridData = new CubeData[totalRows, columns];
            _columnBottom = new int[columns];
            _columnTop = new int[columns];

            //YENI:
            //_targetSelector = targetSelector;

            for (int row = 0; row < totalRows; row++)
                for (int col = 0; col < columns; col++)
                    _gridData[row, col] = new CubeData(col, rows[row].colors[col]);

            for (int col = 0; col < columns; col++)
            {
                _columnBottom[col] = 0;
                _columnTop[col] = totalRows;
            }
        }

        public bool HasValidTarget(int col, CubeColor color)
        {
            if (GetColumnHeight(col) == 0) return false;
            return _gridData[_columnBottom[col], col].Color == color;
        }

        public void LogicalHit(int col)
        {
            _columnBottom[col]++;
        }

        public int GetColumnHeight(int col)
        {
            return _columnTop[col] - _columnBottom[col];
        }

        public int GetColumnBottom(int col) => _columnBottom[col];
        public int GetColumnTop(int col) => _columnTop[col];

        public CubeData GetDataAt(int dataRow, int col)
        {
            return _gridData[dataRow, col];
        }
        /*yeni
         
        public int RequestShot(CubeColor color)
        {
            TargetResult result = _targetSelector.FindTarget(color);
            if (!result.HasTarget) return -1;

            LogicalHit(result.Column); // pre-emptive hit


            return result.Column;
        }
        */



        /*
        public void SetDataAt(int dataRow, int col, CubeColor color)
        {
            _gridData[dataRow, col] = color;
        }*/
    }
}