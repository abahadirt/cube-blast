using Blast.Core.Data;
using System;

namespace Blast.Core.Logic
{
    public class ShooterLogic
    {
        private readonly ShooterData _data;

        public ShooterLogic(CubeColor color, int ammo)
        {
            _data = new ShooterData(color, ammo);
        }


        // Sadece okuma eriþimleri — Data'nýn gerekli alanlarýný expose et
        public CubeColor Color => _data.Color;
        public int Ammo => _data.Ammo;
        public bool IsActive => _data.IsActive;
        public bool IsDepleted => _data.IsDepleted;

        // --- Hesaplamalý alanlar (Logic'te olmalý) ---
        public bool IsOnCooldown => _data.CooldownRemaining > 0f;
        public bool CanFire => IsActive && Ammo > 0 && !IsOnCooldown;

        // Dýþ dünyanýn merminin bittiðini anlamasý için event
        public event Action Depleted;

        // State deðiþtirme — hepsi Logic üzerinden
        public void Tick(float deltaTime)
        {
            if (_data.CooldownRemaining > 0f)
            {
                _data.CooldownRemaining -= deltaTime;
                if (_data.CooldownRemaining < 0f) _data.CooldownRemaining = 0f;
            }
        }

        public void Activate()
        {
            _data.IsActive = true;
            _data.IsDepleted = false;
        }

        public void Deactivate() => _data.IsActive = false;

        private void ConsumeAmmo()
        {
            if (_data.Ammo <= 0) return;

            _data.Ammo--;
            _data.CooldownRemaining = _data.FireCooldown;

            if (_data.Ammo == 0)
            {
                _data.IsActive = false;
                _data.IsDepleted = true;
            }
        }

        public void AddAmmo(int amount) => _data.Ammo += amount;

        public bool Fire()
        {
            if (!CanFire) return false;

            ConsumeAmmo();

            if (_data.IsDepleted)
            {
                Depleted?.Invoke(); // Mermi bittiyse dýþarýya baðýr
            }

            return true;
        }
    }
}

/*
using Blast.Core.Data;
using System;

namespace Blast.Core.Logic
{
    public class ShooterLogic
    {
        private readonly ShooterData _data;

        public ShooterLogic(CubeColor color, int ammo)
        {
            _data = new ShooterData(color, ammo);
        }

        // Sadece okuma eriþimleri — Data'nýn gerekli alanlarýný expose et
        public CubeColor Color => _data.Color;
        public int Ammo => _data.Ammo;
        public bool IsActive => _data.IsActive;
        public bool IsDepleted => _data.IsDepleted;
        public bool CanFire => _data.CanFire;

        // State deðiþtirme — hepsi Logic üzerinden
        public void Activate() => _data.Activate();
        public void Deactivate() => _data.Deactivate();
        public bool TryFire() => _data.TryFire();
        public void AddAmmo(int amount) => _data.AddAmmo(amount);
        public void Tick(float deltaTime) => _data.Tick(deltaTime);


        }
}
*/