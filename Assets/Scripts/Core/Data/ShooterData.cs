namespace Blast.Core.Data
{
    public class ShooterData
    {
        public CubeColor Color { get; }
        public int Ammo { get;  set; }
        public bool IsActive { get;  set; }
        public bool IsDepleted { get;  set; }

        // public bool CanFire => IsActive && Ammo > 0;

        // --- yeni alanlar ---
        public float FireCooldown { get; }              // her atýþtan sonra beklenecek süre
        public float CooldownRemaining { get;  set; }

        public ShooterData(CubeColor color, int ammo, float fireCooldown = 0.3f)
        {
            Color = color;
            Ammo = ammo;
            FireCooldown = fireCooldown;
            CooldownRemaining = 0f;
        }

    }
}



/*

namespace Blast.Core.Data
{
    public class ShooterData
    {
        public CubeColor Color { get; }
        public int Ammo { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsDepleted { get; private set; }

        // public bool CanFire => IsActive && Ammo > 0;

        // --- yeni alanlar ---
        public float FireCooldown { get; }              // her atýþtan sonra beklenecek süre
        public float CooldownRemaining { get; private set; }


        public bool IsOnCooldown => CooldownRemaining > 0f;
        public bool CanFire => IsActive && Ammo > 0 && !IsOnCooldown;
        public ShooterData(CubeColor color, int ammo, float fireCooldown = 0.3f)
        {
            Color = color;
            Ammo = ammo;
            FireCooldown = fireCooldown;
            CooldownRemaining = 0f;
        }
        // --- yeni metod ---
        public void Tick(float deltaTime)
        {
            if (CooldownRemaining > 0f)
            {
                CooldownRemaining -= deltaTime;
                if (CooldownRemaining < 0f) CooldownRemaining = 0f;
            }
        }

        public void Activate() { IsActive = true; IsDepleted = false; }
        public void Deactivate() => IsActive = false;
        public void ConsumeAmmo()
        {
            if (Ammo <= 0) return;
            Ammo--;
            CooldownRemaining = FireCooldown;
            if (Ammo == 0)
            {
                IsActive = false;
                IsDepleted = true;
            }
        }
        public void AddAmmo(int amount) => Ammo += amount;

        public bool TryFire()
        {
            if (!CanFire) return false;
            ConsumeAmmo();
            return true;
        }
    }
}

*/