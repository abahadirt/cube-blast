using System.Collections.Generic;

namespace Blast.GamePresentation.Contract
{
    public interface ILaunchTrayView
    {
        void PlayArrivalAnimation(int shooterId, int slotIndex, float duration);
        void PlayMergeAnimation(int survivorShooterId, int consumedShooterId1, int consumedShooterId2, int totalAmmo);
        void UpdateShooterAmmo(int shooterId, int ammo);
        void PlayDepartureAnimation(int shooterId);
    }

}

