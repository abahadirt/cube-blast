using Blast.Core.Logic;
using System.Collections.Generic;
using Blast.GamePresentation.Contract;


namespace Blast.GamePresentation.Presenter
{

    public class LaunchTrayPresenter
    {
        private readonly LaunchTrayLogic _logic;
        private readonly ILaunchTrayView _view;

        public LaunchTrayPresenter(LaunchTrayLogic logic, ILaunchTrayView view)
        {
            _logic = logic;
            _view = view;
        }

        public void ReceiveShooter(int objectId, int slotIndex, float duration)
        {
            _view.PlayArrivalAnimation(objectId, slotIndex, duration);
        }
        public void MergeShooters(int survivorId, IReadOnlyList<int> consumedIds, int totalAmmo)
        {
            _view.PlayMergeAnimation(survivorId, consumedIds, totalAmmo);
        }

        public void TempResolveShooterFired(int shooterId, int remainingAmmo)
        {
            _view.TempUpdateShooterAmmo(shooterId, remainingAmmo);
        }





    }

}

