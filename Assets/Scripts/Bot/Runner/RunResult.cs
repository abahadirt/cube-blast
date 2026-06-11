namespace Blast.Bot.Runner
{
    public enum RunOutcome { Win, Lose, Timeout }

    /// <summary>
    /// Result and summary metrics produced by a single simulation run.
    /// </summary>
    public sealed class RunResult
    {
        public string LevelId;
        public string Policy;
        public int Seed;

        public RunOutcome Outcome;
        public int Ticks;
        public float SimTime;

        public int TotalTaps;            // non-null decisions the policy made
        public int TotalSends;           // taps that actually moved a shooter to the tray (ShooterSentEvent)
        public int TotalShots;           // ShooterFiredEvent count
        public int TotalMerges;          // ShootersMergedEvent count
        public int TrayFullStalls;       // taps that no-op'd (TotalTaps - TotalSends)

        public int TotalCubesCleared;    // board cubes removed over the run (initial - final)
        public int FinalCubesRemaining;  // board cubes left at resolution (0 on a Win)
        public bool ReserveExhausted;    // reserve held no shooters at resolution

        public string Fingerprint;       // deterministic hash of the event stream (same seed -> same value)
    }
}
