using Blast.Core.Logic;
using Blast.GamePresentation.Contract;

namespace Blast.GamePresentation.Presenter
{
    public class ShooterReservePresenter
    {

        private readonly ShooterReserveLogic _logic;
        private readonly IShooterReserveView _view;

        public ShooterReservePresenter(ShooterReserveLogic logic, IShooterReserveView view)
        {
            _logic = logic;
            _view = view;
        }




        public void Initialize()
        {
            _view.BuildColumns(_logic.GetInitialState());

        }

        public void ReleaseShooter(int columnIndex)
        {
            _view.DetachFirstInColumn(columnIndex);
            _view.PlayShiftAnimation(columnIndex);
        }





    }
}
