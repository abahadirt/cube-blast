using Blast.Core.Data;
using System;
namespace Blast.Core.Logic
{
    public class  LaunchTraySlotLogic
    {
        
        private LaunchTraySlotData _data;
        public ShooterLogic ShooterLogic { get; private set; }

        // Expose
        public bool IsAvailable => _data.IsEmpty; 
        public bool HasArrived => _data.HasArrived; 
        public float ArrivalProgress => _data.ArrivalProgress;
        public float ArrivalDuration => _data.arrivalDuration;

        public LaunchTraySlotLogic(LaunchTraySlotData data)
        {
            _data = data;
        }
        public void AssignShooter(ShooterLogic shooterLogic, float arrivalDuration)
        {
            ShooterLogic = shooterLogic;

            _data.AssignedShooterId = shooterLogic.Id;

            _data.HasArrived = false;

            //Tick için yeni
            _data.arrivalDuration = arrivalDuration;
            _data.arrivalElapsed = 0f;
            _data.ArrivalProgress = 0f;
            ShooterLogic.Depleted += OnShooterDepleted;
        }


        // Shooter'in slota yerleþme surecini yonetir.
        public void Tick(float dt)
        {
            if (_data.HasArrived || _data.IsEmpty) return;

            _data.arrivalElapsed += dt;
            _data.ArrivalProgress = Math.Clamp(_data.arrivalElapsed / _data.arrivalDuration, 0f, 1f);

            if (_data.arrivalElapsed >= _data.arrivalDuration)
            {
                _data.HasArrived = true;
            }
        }


        private void OnShooterDepleted()
        {
            Clear();
        }

        public void Clear()
        {

            if (ShooterLogic != null)
            {
                ShooterLogic.Depleted -= OnShooterDepleted;
            }

            ShooterLogic = null;
            _data.AssignedShooterId = null;

            _data.HasArrived = false;
            _data.ArrivalProgress = 0f;
            _data.arrivalElapsed = 0f;

        }

    }
}