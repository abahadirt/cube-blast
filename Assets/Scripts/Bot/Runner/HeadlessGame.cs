using Blast.Core.Config;
using Blast.Core.Event;
using Blast.Core.Logic;
using Blast.Level;

namespace Blast.Bot.Runner
{
    public sealed class HeadlessGame
    {
        public GameEventQueue Queue { get; }
        public BoardLogic Board { get; }
        public LaunchTrayLogic Tray { get; }
        public ShooterReserveLogic Reserve { get; }
        public GameplayLogic Gameplay { get; }
        public int Columns { get; }
        public int VisibleRows { get; }

        public HeadlessGame(LevelData level, CoreConfig config)
        {
            Queue = new GameEventQueue();
            Board = new BoardLogic(level.columns, level.totalRows, level.rows);
            Tray = new LaunchTrayLogic(level.launchTrayCapacity, Queue, config);
            Reserve = new ShooterReserveLogic(level.reserveColumns, config);
            var targets = new TargetSelector(Board);
            var fire = new FireCoordinator(targets, Tray, Board, Queue);
            var eval = new LevelConditionEvaluator(Board, Tray, Reserve, Queue);
            Gameplay = new GameplayLogic(Board, Tray, Reserve, targets, fire, Queue, eval);
            Columns = level.columns;
            VisibleRows = level.visibleRows;
        }
    }
}
