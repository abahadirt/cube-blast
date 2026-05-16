using Blast.Core.Data;
using System.Collections.Generic;

// Boxing muhabbetinden struct yerine class kullanýldý.
// Poola geçilirse en avantajlýsý class oluyor: no GC, no boxing...
// (her event türü için ayrý queue kullanmak istemedim) tek queue'da kod daha yakýþýklý

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
        public float ArrivalDuration { get; } // TODO[P4]: suan statik deger donuyor ilerde bir ihtimal mesafeye gore dinamik hesaplanabilir. 
        

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
        //public int SurvivorSlotIndex { get; }
        public int SurvivorShooterId { get; }

        public IReadOnlyList<int> ConsumedShooterIds { get; }

        public int TotalAmmo { get; }

        public ShootersMergedEvent(int survivorShooterId, List<int> consumedShooterIds, int totalAmmo)
        {
            SurvivorShooterId = survivorShooterId;
            ConsumedShooterIds = consumedShooterIds.ToArray(); //TODO[P?] gerçekten immutable mý ve best practice mi emin degilim.
            TotalAmmo = totalAmmo;
        }
    }
    // bura degisebilir
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
    // bura degisebilir
    public class CubeHitEvent : IGameEvent
    {
        public int Column { get; }

        public int DestroyedLogicalRow { get; }
        public bool IsDestroyed { get; }

        public CubeHitEvent(int column, int destroyedLogicalRow, bool isDestroyed)
        {
            Column = column;
            DestroyedLogicalRow = destroyedLogicalRow;
            IsDestroyed = isDestroyed;
        }
    }



}