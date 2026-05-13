using System.Collections.Generic;

namespace Blast.GamePresentation.Contract
{
    public interface ILaunchTrayView
    {
        void PlayArrivalAnimation(int shooterId, int slotIndex, float duration);
        void PlayMergeAnimation(int survivorShooterID, IReadOnlyList<int> consumedShooterIds, int totalAmmo);
        void TempUpdateShooterAmmo(int shooterId, int ammo);
    }

}

