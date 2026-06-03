namespace Blast.Core.Config
{
    // Game-affecting tuning parameters only, injected by each composition root
    public sealed class CoreConfig
    {
        // <summary>Seconds a shooter waits between shots.
        public const float DefaultFireCooldown = 0.2f;

        // Seconds a shooter takes to arrive in a tray slot. 
        public const float DefaultArrivalDuration = 0.15f;

        public float FireCooldown { get; }
        public float ArrivalDuration { get; }

        public CoreConfig(
            float fireCooldown = DefaultFireCooldown,
            float arrivalDuration = DefaultArrivalDuration)
        {
            FireCooldown = fireCooldown;
            ArrivalDuration = arrivalDuration;
        }
    }
}
