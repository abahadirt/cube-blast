namespace Blast.Core.Data
{
    public class ShooterData
    {
        public int Id { get; }
        public CubeColor Color { get; }
        public int Ammo { get;  set; }
        public bool IsActive { get;  set; }
        public bool IsDepleted { get;  set; }

        public float FireCooldown { get; }  // Cooldown duration after each shot.
        public float CooldownRemaining { get;  set; }

        // TODO[P1]: Review
        public ShooterData(int id,CubeColor color, int ammo, float fireCooldown)
        {
            Id = id;
            Color = color;
            Ammo = ammo;
            FireCooldown = fireCooldown;
            CooldownRemaining = 0f;
        }

    }
}



