using Blast.Core.Config;
using Blast.Core.Data;
using Blast.Logging;
using System.Collections.Generic;

namespace Blast.Core.Logic
{
    public class ShooterReserveLogic
    {
        // Reserve columns' elements are consumed in FIFO order.
        // List is used instead of Queue to support future features requiring index-based access.
        private readonly CoreConfig _config;
        private List<List<ShooterLogic>> _reserveColumns;
        public ShooterReserveLogic(List<ReserveColumnData> reserveColumns, CoreConfig config)
        {
            _config = config;
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
                    var shooter = new ShooterLogic(GenerateShooterId(), shooterData.color, shooterData.ammo, _config.FireCooldown);
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




        //TODO[P3]: Review this approach.
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

        
        public int ColumnCount => _reserveColumns.Count;
        
        public int GetColumnLength(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= _reserveColumns.Count) return 0;
            return _reserveColumns[columnIndex].Count;
        }
        
        public bool TryPeek(int columnIndex, int depth, out CubeColor color, out int ammo)
        {
            color = default;
            ammo = 0;
            // Check if column exists
            if (columnIndex < 0 || columnIndex >= _reserveColumns.Count) return false;
            var column = _reserveColumns[columnIndex];
            // Check if depth is within bounds
            if (depth < 0 || depth >= column.Count) return false;
            var shooter = column[depth];
            color = shooter.Color;
            ammo = shooter.Ammo;
            return true;
        }








    }
}