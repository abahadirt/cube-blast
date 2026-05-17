using Blast.Core.Data;
using Blast.Core.Event;
using System.Collections.Generic;
using Blast.Logging;
namespace Blast.Core.Logic
{
    
    public class MergeResult
    {
        public bool IsMerged { get; set; }
        public int SurvivorIndex { get; set; }
        public List<int> ConsumedIndices { get; set; }
        public int TotalBonusAmmo { get; set; }
    }

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

        // nullsa gönderme vs gereksiz ayrýntý...
        public List<MergeResult> Tick(float deltaTime)
        {
            for (int i = 0; i < slotLogics.Length; i++)
            {
                slotLogics[i].Tick(deltaTime);
            }
            // TODO[P2] : TryMergeAll' early exit eklenecek. local parametre isimleri daha anlaþýlýr yapýlacak. 
            // TODO[P3] : Her tick'te merge kontrolü yapýyor -> bakýlacak. refactor yaparken mid loop mutation dikkate alýnacak.
            return TryMergeAll(); 
        }

        public int AddShooter(ShooterLogic shooter, float arrivalDuration=0.15f)
        {
            int slotIndex = GetAvailableSlotIndex();
            if (slotIndex == -1) return -1;
            
            RegisterShooterAt(slotIndex, shooter, arrivalDuration);
            // ShooterSentEvent iki farkli logic elemaniyla gerceklestirildiginden, event gameplay logicte queue'ya ekleniyor. 
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


        private List<MergeResult> TryMergeAll()
        {
            List<MergeResult> results = null;

            // Tray'de hangi renkler arrive olmuþ halde var, onlarý topla
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


            // Her renk için merge kontrolü
            foreach (var kvp in slotsByColor)
            {
                var matchingSlots = kvp.Value;

                if (matchingSlots.Count < 3)
                    continue;




                //AAEVENT
                List<int> consumedShooterIds = new List<int>();

                // Orijinal kuraldaki gibi: listenin 2. elemaný (index 1) hayatta kalýyor
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

                if (results == null)
                    results = new List<MergeResult>();
                Log.Info(nameof(LaunchTrayLogic), $"Merge oldu! SurvivorShooterId: {survivorShooterId}, ConsumedShooterIds: {string.Join(", ", consumedShooterIds)}, TotalBonusAmmo: {bonusAmmo}");
                _eventQueue.Enqueue(new ShootersMergedEvent(survivorShooterId, consumedShooterIds, slotLogics[survivorIndex].ShooterLogic.Ammo));

                results.Add(new MergeResult
                {
                    IsMerged = true,
                    SurvivorIndex = survivorIndex,
                    ConsumedIndices = consumedSlots,
                    TotalBonusAmmo = bonusAmmo
                });
            }

            ActivateAll();
            return results;
        }


        // TODO[P2]: çaðrýldýðý yer ve kendisi baþka þekilde ele alýnacak .
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


        /*
        public MergeResult ExecuteMerge(CubeColor color)
        {
            List<int> matchingSlots = new List<int>();

            for (int i = 0; i < slotLogics.Length; i++)
            {
                var slot = slotLogics[i];
                if (!slot.IsAvailable && slot.HasArrived && slot.ShooterLogic.Color == color)
                {
                    matchingSlots.Add(i);
                }
            }

            if (matchingSlots.Count < 3)
            {
                return new MergeResult { IsMerged = false };
            }

            // Orijinal kuraldaki gibi: listendeki 2. eleman (index 1) hayatta kalýyor.
            int survivorIndex = matchingSlots[1];
            matchingSlots.RemoveAt(1);
            List<int> consumedSlots = matchingSlots;

            int bonusAmmo = 0;

            // Arkaplanda mermileri topla ve Data'daki slotlarý hemen temizle
            foreach (int index in consumedSlots)
            {
                bonusAmmo += slotLogics[index].ShooterLogic.Ammo;
                slotLogics[index].Clear(); // Logic seviyesinde slot temizlendi.
            }

            // Hayatta kalan mantýksal modele (ShooterLogic) mermileri ekle.
            slotLogics[survivorIndex].ShooterLogic.AddAmmo(bonusAmmo);
            

            return new MergeResult
            {
                IsMerged = true,
                SurvivorIndex = survivorIndex,
                ConsumedIndices = consumedSlots,
                TotalBonusAmmo = bonusAmmo
            };
        }
        */


    }
}





