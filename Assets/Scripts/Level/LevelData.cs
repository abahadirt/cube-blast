using System.Collections.Generic;

using Blast.Core.Data;


namespace Blast.Level
{
    [System.Serializable]
    public class LevelData
    {
        // BoardLogic için
        public int columns { get; set; }
        public int totalRows { get; set; }
        public GridRow[] rows { get; set; }

        // LaunchTrayLogic için
        public int launchTrayCapacity { get; set; }

        // ShooterReserveLogic için
        public List<ReserveColumnData> reserveColumns { get; set; }

        // BoardPresenter için
        public int visibleRows { get; set; }


    }

}

