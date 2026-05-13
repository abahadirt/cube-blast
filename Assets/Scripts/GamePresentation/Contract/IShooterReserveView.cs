using System.Collections.Generic;
using Blast.Core.Data;

namespace Blast.GamePresentation.Contract
{
    public interface IShooterReserveView
    {
        void BuildColumns(IReadOnlyList<IReadOnlyList<ShooterData>> columnsData);
        void DetachFirstInColumn(int columnIndex);
        void PlayShiftAnimation(int columnIndex);
    }
}