using Blast.Core.Data;
using System.Collections.Generic;

namespace Blast.Core.Logic
{

    // farklı küp turleri eklenince ihtiyaç olacak.
    public readonly struct TargetResult
    {
        public readonly bool HasTarget;
        public readonly int Column;
        public readonly int Row;
        public TargetResult(bool hasTarget, int column, int row)
        {
            HasTarget = hasTarget;
            Column = column;
            Row = row;
        }

        public static TargetResult None => new TargetResult(false, -1,-1);
    }

    public class TargetSelector
    {
        private readonly BoardLogic _board;
        private readonly Dictionary<CubeColor, int> _colorMemory = new();

        public TargetSelector(BoardLogic board)
        {
            _board = board;
        }


        // TODO[P1]: Single Resp. bozuyor, yaklaşım review edilecek, metod isim güncellenebilir.
        public TargetResult FindTarget(CubeColor color)
        {
            if (!_colorMemory.ContainsKey(color))
                _colorMemory[color] = -1;

            int colCount = _board.Columns;
            int startCol = (_colorMemory[color] + 1) % colCount;

            for (int i = 0; i < colCount; i++)
            {
                int currentCol = (startCol + i) % colCount;
                if (_board.HasValidTarget(currentCol, color))
                {
                    _colorMemory[color] = currentCol;
                    int hitRow = _board.GetColumnBottom(currentCol);
                    _board.LogicalHit(currentCol); // pre emptive hit
                    return new TargetResult(true, currentCol,hitRow);
                }
            }

            return TargetResult.None;
        }






    }
}