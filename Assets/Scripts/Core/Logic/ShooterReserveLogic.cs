using System.Collections.Generic;
using Blast.Core.Data;
using Blast.Logging;

namespace Blast.Core.Logic
{
    public class ShooterReserveLogic
    {
        // Reserve kolonlarý FIFO sýrasýyla tüketilir.
        // Farklý ve ayný kolondaki shooter'lar arasýnda indeks bazlý eriþim gereken feature ekleneceði için Queue yerine List kullanýlýr.
        private List<List<ShooterLogic>> _reserveColumns;
        public ShooterReserveLogic(List<ReserveColumnData> reserveColumns)
        {
            InitializeFromLevelData(reserveColumns);
        }

        private void InitializeFromLevelData(List<ReserveColumnData> reserveColumnsData)
        {
            _reserveColumns = new List<List<ShooterLogic>>(reserveColumnsData.Count);
            Log.Info(nameof(ShooterReserveLogic), $"Initializing columns in reserve logic with {reserveColumnsData.Count} columns from level data.");

            foreach (var columnData in reserveColumnsData)
            {
                var columnQueue = new List<ShooterLogic>(columnData.shooters.Count);
                Log.Info(nameof(ShooterReserveLogic), $"Initializing shooter column with {columnData.shooters.Count} shooters.");
                foreach (var shooterData in columnData.shooters)
                {
                    var shooter = new ShooterLogic(GenerateShooterId(), shooterData.color, shooterData.ammo);
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


        private int _nextShooterIdToAssign = 1;
        private int GenerateShooterId()
        {
            return _nextShooterIdToAssign++;
        }




        //TODO[P3]: yaklaþým review edilecek.
        public List<List<ShooterData>> GetInitialState()
        {
            var initialState = new List<List<ShooterData>>(_reserveColumns.Count);
            foreach (var column in _reserveColumns)
            {
                var columnData = new List<ShooterData>(column.Count);
                foreach (var shooterLogic in column)
                {
                    var shooter = new ShooterData(shooterLogic.Id, shooterLogic.Color, shooterLogic.Ammo, shooterLogic.FireCooldown);
                    columnData.Add(shooter);
                }
                initialState.Add(columnData);
            }
            return initialState;

        }
        public bool IsEmpty()
        {
            for (int i = 0; i < _reserveColumns.Count; i++)
                if (_reserveColumns[i].Count > 0) return false;
            return true;
        }

    }
}