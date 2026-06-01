namespace Blast.Core.Data
{
    public class LaunchTraySlotData
    {
        public int? AssignedShooterId { get; set; } = null;
        public bool HasArrived { get;  set; }

        //Tick icin yeni
        public float ArrivalProgress { get;  set; } // 0..1, presentation için
        public float arrivalDuration;
        public float arrivalElapsed;

        public bool IsEmpty => AssignedShooterId == null;
    }
}