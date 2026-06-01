namespace Blast.Core.Data
{
    public class ShooterData
    {
        public int Id { get; }
        public CubeColor Color { get; }
        public int Ammo { get;  set; }
        public bool IsActive { get;  set; }
        public bool IsDepleted { get;  set; }

        public float FireCooldown { get; }              // her atıştan sonra beklenecek süre
        public float CooldownRemaining { get;  set; }

        public ShooterData(int id,CubeColor color, int ammo, float fireCooldown = 0.2f)
        {
            Id = id;
            Color = color;
            Ammo = ammo;
            FireCooldown = fireCooldown;
            CooldownRemaining = 0f;
        }

    }
}



