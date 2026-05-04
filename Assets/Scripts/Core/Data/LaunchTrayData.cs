using Blast.Core.Logic;
using System;
namespace Blast.Core.Data
{

    public class LaunchTrayData
    {
        public int Capacity { get; }
        public LaunchTraySlotData[] Slots { get; }

        public LaunchTrayData(int capacity)
        {
            Capacity = capacity;
            Slots = new LaunchTraySlotData[capacity];
            for (int i = 0; i < capacity; i++)
            {
                Slots[i] = new LaunchTraySlotData();
            }
        }
    }
}