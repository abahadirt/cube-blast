using Blast.Core.Logic;
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
        public void MergeShooters(int survivorId, int consumedId1, int consumedId2, int totalAmmo)
        {
            _view.PlayMergeAnimation(survivorId, consumedId1, consumedId2, totalAmmo);
        }

        public void ResolveShooterFired(int shooterId, int remainingAmmo)
        {
            _view.UpdateShooterAmmo(shooterId, remainingAmmo);
            if (remainingAmmo <= 0)
            {
                _view.PlayDepartureAnimation(shooterId);
            }
        }





    }

}

