using Blast.Bot.Observation;

namespace Blast.Bot.Runner
{
    /// <summary>
    /// Configuration for fixed-step headless simulations.
    /// </summary>
    public sealed class SimConfig
    {
        public float Dt = 1f / 60f;
        public int MaxTicks = 100_000;
        public int DecisionEveryNTicks = 6;
        public ObservationMode Mode = ObservationMode.Oracle;
    }
}
