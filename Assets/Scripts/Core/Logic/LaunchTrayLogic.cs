using Blast.Core.Config;
using Blast.Core.Data;
using Blast.Core.Event;
using Blast.Logging;
using System;

namespace Blast.Core.Logic
{
    public class LaunchTrayLogic
    {
        private readonly CoreConfig _config;
        private readonly GameEventQueue _eventQueue;
        private readonly LaunchTrayData _data;

        public LaunchTraySlotLogic[] slotLogics;

        private static readonly int ColorCount = Enum.GetValues(typeof(CubeColor)).Length;
        public LaunchTrayLogic(int capacity, GameEventQueue eventQueue, CoreConfig config)
        {
            _config = config;
            _eventQueue = eventQueue;
            _data = new LaunchTrayData(capacity);

            slotLogics = new LaunchTraySlotLogic[capacity];
            for (int i = 0; i < capacity; i++)
            {
                slotLogics[i] = new LaunchTraySlotLogic(_data.Slots[i]);
            }

        }

        public void Tick(float deltaTime)
        {
            for (int i = 0; i < slotLogics.Length; i++)
            {
                slotLogics[i].Tick(deltaTime);
            }        
            TryMergeAll(); 
        }

        public int AddShooter(ShooterLogic shooter)
        {
            int slotIndex = GetAvailableSlotIndex();
            if (slotIndex == -1) return -1;
            
            RegisterShooterAt(slotIndex, shooter, _config.ArrivalDuration);
            // ShooterSentEvent spans multiple logic elements, so it is queued at the gameplaylogic.
            return slotIndex;
        }

        public bool HasSpace()
        {
            for (int i = 0; i < slotLogics.Length; i++)
            {
                if (slotLogics[i].IsAvailable) return true;
            }
            return false;
        }

        public int GetAvailableSlotIndex()
        {
            for (int i = 0; i < slotLogics.Length; i++)
            {
                if (slotLogics[i].IsAvailable) return i;
            }
            return -1;
        }

        public void RegisterShooterAt(int index, ShooterLogic shooterLogic, float arrivalDuration)
        {
            if (index >= 0 && index < slotLogics.Length)
            {
                slotLogics[index].AssignShooter(shooterLogic, arrivalDuration);
            }
        }


        public void ClearSlot(int index)
        {
            if (index >= 0 && index < slotLogics.Length)
            {
                slotLogics[index].Clear();
            }
        }

        /// <summary>
        /// Scans the tray to find and merge exactly three arrived shooters of the same color.
        /// Highly optimized for zero-allocation (GC-friendly) performance during frequent calls:
        /// 
        /// 1. Stack Allocation (`stackalloc`): Uses the stack instead of the heap for temporary arrays, 
        ///    completely eliminating Garbage Collection (GC) overhead and preventing micro-stutters.
        ///    
        /// 2. 1D Array as 2D Array: Instead of allocating complex nested lists or 2D arrays 
        ///    (e.g., [Color][SlotIndex]), it uses a flat, continuous 1D array (`firstThree`). 
        ///    The formula `color_index * 3 + current_count` maps the 2D logic directly into 1D space.
        ///    
        /// 3. Early Exit: Tracks `totalArrived` items and bypasses the merge loop entirely 
        ///    if there are fewer than 3 items on the tray.
        /// </summary>
        private void TryMergeAll()
        {
            const int TripleSize = 3;

            // stack implementation:
            Span<int> count = stackalloc int[ColorCount];
            Span<int> firstThree = stackalloc int[ColorCount * TripleSize];
            int totalArrived = 0;

            for (int i = 0; i < slotLogics.Length; i++)
            {
                LaunchTraySlotLogic slot = slotLogics[i];
                if (slot.IsAvailable || !slot.HasArrived) continue;

                int c = (int)slot.ShooterLogic.Color;
                if (count[c] < TripleSize)               // after 3, don't care about the rest for merging
                    firstThree[c * TripleSize + count[c]] = i;

                count[c]++;
                totalArrived++;
            }

            // Early exit: if less than 3 arrived elements. skip merge.
            // ActivateAll() koşulun dışında, her tick çalışmalı.
            if (totalArrived >= 3)
            {
                for (int c = 0; c < ColorCount; c++)
                {
                    if (count[c] < TripleSize) continue;

                    int row = c * TripleSize;
                    // middle (firstThree[row + 1]) survives.
                    MergeTriple(firstThree[row], firstThree[row + 1], firstThree[row + 2]);
                }
            }

            ActivateAll();
        }

        // Middle slot element survives and absorbs ammo from the other two.
        private void MergeTriple(int lowIndex, int midIndex, int highIndex)
        {
            ShooterLogic survivor = slotLogics[midIndex].ShooterLogic;
            ShooterLogic consumedLow = slotLogics[lowIndex].ShooterLogic;
            ShooterLogic consumedHigh = slotLogics[highIndex].ShooterLogic;

            int survivorId = survivor.Id;
            int consumedId1 = consumedLow.Id;
            int consumedId2 = consumedHigh.Id;
            int bonusAmmo = consumedLow.Ammo + consumedHigh.Ammo;

            slotLogics[lowIndex].Clear();
            slotLogics[highIndex].Clear();

            survivor.AddAmmo(bonusAmmo);

            Log.Info(nameof(LaunchTrayLogic),
                $"Merge successful! Survivor: {survivorId}, Consumed: {consumedId1}, {consumedId2}, TotalAmmo: {survivor.Ammo}");

            _eventQueue.Enqueue(new ShootersMergedEvent(survivorId, consumedId1, consumedId2, survivor.Ammo));
        }
       

        // TODO[P2]: Review usage and implementation.
        public void ActivateAll()
        {
            foreach (var slot in slotLogics)
            {
                var shooter = slot.ShooterLogic;

                if (shooter == null) continue;
                
                if (!shooter.IsDepleted && slot.HasArrived)
                {
                    shooter.Activate();
                }
            }
        }


    }
}





