namespace Blast.Core.Data
{
    public class LaunchTraySlotData
    {
        public int? AssignedShooterId { get; set; } = null;
        public bool HasArrived { get;  set; }

        public float ArrivalProgress { get;  set; } // 0 to 1, exposed for presentation purposes
        public float arrivalDuration;
        public float arrivalElapsed;

        public bool IsEmpty => AssignedShooterId == null;
    }
}