using Blast.Core.Data;

// Classes used instead of structs to prevent boxing.
// Beneficial if object pooling is added later (no GC, no boxing).
// A single queue for all event types is preferred for cleaner architecture.

// UNITY DOCUMENT: """...To get around this, you should try to reduce the amount of
// frequently managed heap allocations as possible: ideally to 0 bytes per
// frame, or as close to !!!zero!!! as you can get."

namespace Blast.Core.Event
{
    public class ShooterSentEvent : IGameEvent
    {
        public int ShooterId { get; }
        public int TargetSlotIndex { get; }
        public int SourceColumnIndex { get; }
        public float ArrivalDuration { get; } // TODO[P99]: Re-evaluate static value; could be dynamically calculated based on distance later.


        public ShooterSentEvent(int shooterId, int targetSlotIndex, int sourceColumnIndex, float arrivalDuration)
        {
            ShooterId = shooterId;
            TargetSlotIndex = targetSlotIndex;
            SourceColumnIndex = sourceColumnIndex;
            ArrivalDuration = arrivalDuration;
        }
    }

    public class ShootersMergedEvent : IGameEvent
    {
        public int SurvivorShooterId { get; }
        public int ConsumedShooterId1 { get; }
        public int ConsumedShooterId2 { get; }
        public int TotalAmmo { get; }

        public ShootersMergedEvent(int survivorShooterId, int consumedShooterId1, int consumedShooterId2, int totalAmmo)
        {
            SurvivorShooterId = survivorShooterId;
            ConsumedShooterId1 = consumedShooterId1;
            ConsumedShooterId2 = consumedShooterId2;
            TotalAmmo = totalAmmo;
        }
    }
    // TODO[P0]: review
    public class ShooterFiredEvent : IGameEvent
    {
        public int ShooterId { get; }
        public int SlotIndex { get; }
        public CubeColor Color { get; }
        public int TargetColumn { get; }
        public int TargetLogicalRow { get; }
        
        public int RemainingAmmo { get; }

        public ShooterFiredEvent(int shooterId, int slotIndex, CubeColor color, int targetColumn, int targetLogicalRow, int remainingAmmo)
        {
            ShooterId = shooterId;
            SlotIndex = slotIndex;
            Color = color;
            TargetColumn = targetColumn;
            TargetLogicalRow = targetLogicalRow;
            RemainingAmmo = remainingAmmo;
        }
    }

    public class LevelCompletedEvent : IGameEvent { }

    public class LevelFailedEvent : IGameEvent { }
    
    





}