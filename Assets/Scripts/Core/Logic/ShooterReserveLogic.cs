//FULLY OKEY.

using System.Collections.Generic;
using Blast.Core.Data;

namespace Blast.Core.Logic
{
    public class ShooterReserveLogic
    {
        private List<List<ShooterLogic>> _reserveColumns;
        public ShooterReserveLogic(List<ReserveColumnData> reserveColumns)
        {
            InitializeFromLevelData(reserveColumns);
        }



        private void InitializeFromLevelData(List<ReserveColumnData> reserveColumnsData)
        {
            _reserveColumns = new List<List<ShooterLogic>>(reserveColumnsData.Count);

            foreach (var columnData in reserveColumnsData)
            {
                var columnQueue = new List<ShooterLogic>(columnData.shooters.Count);

                foreach (var shooterData in columnData.shooters)
                {
                    var shooter = new ShooterLogic(shooterData.color, shooterData.ammo);
                    columnQueue.Add(shooter);
                }

                _reserveColumns.Add(columnQueue);
            }
        }


        public ShooterLogic GetNextShooter(int columnIndex)
        {
            if (_reserveColumns[columnIndex].Count == 0) return null;
            var shooter = _reserveColumns[columnIndex][0];
            _reserveColumns[columnIndex].RemoveAt(0);
            return shooter;
        }

    }
}