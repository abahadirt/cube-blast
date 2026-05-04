using Blast.Core.Data;
using System;
namespace Blast.Core.Logic
{
    public class  LaunchTraySlotLogic
    {
        
        
        private LaunchTraySlotData _data;
        public ShooterLogic ShooterLogic => _data.ShooterLogic;

        // Dýþarýdan okuma kolaylýðý için (Opsiyonel)

        public bool IsAvailable => _data.IsAvailable; //TODO[P1] bir iþe yaramýyor. isim kafa karýþtýrýcý, dolu boþ diye tutcaz.
        public bool HasArrived => _data.HasArrived; 
        public float ArrivalProgress => _data.ArrivalProgress;

        public LaunchTraySlotLogic(LaunchTraySlotData data)
        {
            _data = data;
        }
        public void AssignShooter(ShooterLogic shooterLogic, float arrivalDuration)
        {
            _data.ShooterLogic = shooterLogic;
            _data.IsAvailable = false;
            _data.HasArrived = false;

            //Tick için yeni
            _data.arrivalDuration = arrivalDuration;
            _data.arrivalElapsed = 0f;
            _data.ArrivalProgress = 0f;

            _data.ShooterLogic.Depleted += OnShooterDepleted;
        }

        public void Tick(float dt)
        {
            if (_data.HasArrived || _data.IsAvailable) return;

            _data.arrivalElapsed += dt;
            _data.ArrivalProgress = Math.Clamp(_data.arrivalElapsed / _data.arrivalDuration, 0f, 1f);

            if (_data.arrivalElapsed >= _data.arrivalDuration)
            {
                _data.HasArrived = true;
            }
            
        }


        private void OnShooterDepleted()
        {
            _data.ShooterLogic.Depleted -= OnShooterDepleted;
            Clear();
        }

        public void Clear()
        {
            _data.ShooterLogic = null;
            _data.IsAvailable = true;
            _data.HasArrived = false;
            _data.ArrivalProgress = 0f;
            _data.arrivalElapsed = 0f;

        }

    }
}