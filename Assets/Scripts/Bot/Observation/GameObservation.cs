using Blast.Bot.Runner;
using Blast.Core.Data;

namespace Blast.Bot.Observation
{
    /// <summary>
    /// Read-only view of the current game state exposed to bot policies.
    /// </summary>
    public sealed class GameObservation
    {
        private readonly HeadlessGame _game;
        private readonly ObservationMode _mode;
        private readonly int _visibleRows;

        public GameObservation(HeadlessGame game, ObservationMode mode, int visibleRows)
        {
            _game = game;
            _mode = mode;
            _visibleRows = visibleRows;
        }

        // Tray
        public int TrayCapacity => _game.Tray.slotLogics.Length;
        public bool HasTraySpace => _game.Tray.HasSpace();

        public bool SlotOccupied(int i) => !_game.Tray.slotLogics[i].IsAvailable;
        public bool SlotArrived(int i) => _game.Tray.slotLogics[i].HasArrived;

        public CubeColor? SlotColor(int i)
        {
            var slot = _game.Tray.slotLogics[i];
            return (!slot.IsAvailable && slot.ShooterLogic != null) ? (CubeColor?)slot.ShooterLogic.Color : null;
        }

        public int? SlotAmmo(int i)
        {
            var slot = _game.Tray.slotLogics[i];
            return (!slot.IsAvailable && slot.ShooterLogic != null) ? (int?)slot.ShooterLogic.Ammo : null;
        }

        // Reserve
        public const int FairReserveVisibleDepth = 3;
        public int ReserveColCount => _game.Reserve.ColumnCount;

        public int ReserveColLength(int col)
        {
            int n = _game.Reserve.GetColumnLength(col);
            return (_mode == ObservationMode.Fair && n > FairReserveVisibleDepth) ? FairReserveVisibleDepth : n;
        }

        public bool ReserveColHasShooter(int col) => _game.Reserve.GetColumnLength(col) > 0;

        public CubeColor? ReserveColColorAt(int col, int depth)
        {
            if (depth < 0 || depth >= ReserveColLength(col)) return null;
            return _game.Reserve.TryPeek(col, depth, out var color, out _) ? color : (CubeColor?)null;
        }

        public int? ReserveColAmmoAt(int col, int depth)
        {
            if (depth < 0 || depth >= ReserveColLength(col)) return null;
            return _game.Reserve.TryPeek(col, depth, out _, out var ammo) ? (int?)ammo : null;
        }

        // Board

        public int BoardColumns => _game.Board.Columns;
        public bool ColumnHasCube(int c) => _game.Board.GetColumnHeight(c) > 0;
        public int BoardColHeight(int c)
        {
            int h = _game.Board.GetColumnHeight(c);
            return (_mode == ObservationMode.Fair && h > _visibleRows) ? _visibleRows : h;
        }

        public CubeColor? BoardColColorAt(int c, int depth)
        {
            if (c < 0 || c >= _game.Board.Columns) return null;
            if (depth < 0 || depth >= BoardColHeight(c)) return null;

            int dataRow = _game.Board.GetColumnBottom(c) + depth;
            return _game.Board.GetDataAt(dataRow, c).Color;
        }

        public CubeColor? ColumnBottomColor(int c) => BoardColColorAt(c, 0);

    }
}