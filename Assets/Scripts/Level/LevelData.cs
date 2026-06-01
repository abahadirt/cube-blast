using System.Collections.Generic;

using Blast.Core.Data;


namespace Blast.Level
{
    [System.Serializable]
    public class LevelData
    {
        // For BoardLogic
        public int columns { get; set; }
        public int totalRows { get; set; }
        public GridRow[] rows { get; set; }

        // For LaunchTrayLogic
        public int launchTrayCapacity { get; set; }

        // For ShooterReserveLogic
        public List<ReserveColumnData> reserveColumns { get; set; }

        // For BoardPresenter
        public int visibleRows { get; set; }


    }

}

