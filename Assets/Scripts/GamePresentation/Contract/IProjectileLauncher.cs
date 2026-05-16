using System;
using Blast.Core.Data;

namespace Blast.GamePresentation.Contract
{
    public interface IProjectileLauncher
    {
        public void FireFromTrayToBoard(CubeColor color, int sourceSlot, int targetColumn, Action onArrived);
    }

}
