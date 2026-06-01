using Blast.Core.Data;
using Blast.Core.Event;
using System.Collections.Generic;
using Blast.Logging;
namespace Blast.Core.Logic
{
    public class LaunchTrayLogic
    {
        private readonly GameEventQueue _eventQueue;
        private readonly LaunchTrayData _data;

        public LaunchTraySlotLogic[] slotLogics;
        public LaunchTrayLogic(int capacity, GameEventQueue eventQueue)
        {
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
            // TODO[P2]: Add early exit to TryMergeAll. Improve local parameter names.
            // TODO[P3]: Merge check happens every tick; review this. Consider mid-loop mutations during refactor. (after early exit refactor cost will be negligible).
            TryMergeAll(); 
        }

        public int AddShooter(ShooterLogic shooter, float arrivalDuration=0.15f)
        {
            int slotIndex = GetAvailableSlotIndex();
            if (slotIndex == -1) return -1;
            
            RegisterShooterAt(slotIndex, shooter, arrivalDuration);
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


        private void TryMergeAll()
        {
            // Group arrived shooters by color.
            Dictionary<CubeColor, List<int>> slotsByColor = new Dictionary<CubeColor, List<int>>();

            for (int i = 0; i < slotLogics.Length; i++)
            {
                var slot = slotLogics[i];
                if (!slot.IsAvailable && slot.HasArrived)
                {
                    var color = slot.ShooterLogic.Color;
                    if (!slotsByColor.TryGetValue(color, out var list))
                    {
                        list = new List<int>();
                        slotsByColor[color] = list;
                    }
                    list.Add(i);
                }
            }


            // Check for merges per color.
            foreach (var kvp in slotsByColor)
            {
                var matchingSlots = kvp.Value;

                if (matchingSlots.Count < 3)
                    continue;

                //AAEVENT
                List<int> consumedShooterIds = new List<int>();

                // The second element (index 1) survives the merge.
                int survivorIndex = matchingSlots[1];

                //AAEVENT
                int survivorShooterId = slotLogics[survivorIndex].ShooterLogic.Id;

                matchingSlots.RemoveAt(1);
                List<int> consumedSlots = matchingSlots;

                

                int bonusAmmo = 0;
                foreach (int index in consumedSlots)
                {
                    bonusAmmo += slotLogics[index].ShooterLogic.Ammo;
                    //AAEVENT
                    consumedShooterIds.Add(slotLogics[index].ShooterLogic.Id);
                    slotLogics[index].Clear();
                }

                slotLogics[survivorIndex].ShooterLogic.AddAmmo(bonusAmmo);

                Log.Info(nameof(LaunchTrayLogic), $"Merge successful! SurvivorShooterId: {survivorShooterId}, ConsumedShooterIds: {string.Join(", ", consumedShooterIds)}, TotalBonusAmmo: {bonusAmmo}");
                _eventQueue.Enqueue(new ShootersMergedEvent(survivorShooterId, consumedShooterIds, slotLogics[survivorIndex].ShooterLogic.Ammo));
            }

            ActivateAll();
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





