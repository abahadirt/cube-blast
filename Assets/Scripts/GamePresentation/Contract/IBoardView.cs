using Blast.Core.Data;

namespace Blast.GamePresentation.Contract
{
    public interface IBoardView
    {
        void Initialize(int columns, int visibleRows, CubeColor[,] colors, bool[,] activeFlags);
        void RemoveCubeFromBottom(int col, CubeColor? newTopColor);
    }
}