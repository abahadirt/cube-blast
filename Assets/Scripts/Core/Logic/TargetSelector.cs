using Blast.Core.Data;
using System.Collections.Generic;

namespace Blast.Core.Logic
{
    public readonly struct TargetResult
    {
        public readonly bool HasTarget;
        public readonly int Column;

        public TargetResult(bool hasTarget, int column)
        {
            HasTarget = hasTarget;
            Column = column;
        }

        public static TargetResult None => new TargetResult(false, -1);
    }

    public class TargetSelector
    {
        private readonly BoardLogic _board;
        private readonly Dictionary<CubeColor, int> _colorMemory = new();

        public TargetSelector(BoardLogic board)
        {
            _board = board;
        }

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

                    _board.LogicalHit(currentCol); // pre-emptive hit (orijinal davranýþ)
                    return new TargetResult(true, currentCol);
                }
            }

            return TargetResult.None;
        }






    }
}