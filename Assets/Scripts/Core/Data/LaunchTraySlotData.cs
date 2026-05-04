using System;
using Blast.Core.Logic; // HACK
namespace Blast.Core.Data
{
    public class LaunchTraySlotData
    {
        public bool IsAvailable { get;  set; } = true;
        public bool HasArrived { get;  set; }

        //Tick için yeni
        public float ArrivalProgress { get;  set; } // 0..1, presentation için
        public float arrivalDuration;
        public float arrivalElapsed;

        public ShooterLogic ShooterLogic { get;  set; } //TODO[P3]: daha sonra id'ye çevirileceði için logic olarak býrakýldý.


    }
}