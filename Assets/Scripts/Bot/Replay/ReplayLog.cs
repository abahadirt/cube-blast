using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Blast.Bot.Replay
{
    public sealed class TapEntry
    {
        public int tick;
        public int col;
        public TapEntry() { }
        public TapEntry(int tick, int col) { this.tick = tick; this.col = col; }
    }

    public sealed class CoreConfigDto
    {
        public float fireCooldown;
        public float arrivalDuration;
    }

    /// <summary>
    /// Serializable replay data for one headless simulation run.
    /// </summary>
    public sealed class ReplayLog
    {
        public int schemaVersion = 1;
        public string harnessVersion;

        public string levelId;
        public string levelHash;            // SHA-256 of the level JSON — detects content drift
        public CoreConfigDto coreConfig;
        public string policy;
        public string observationMode;      // fair | oracle
        public int seed;
        public float dt;
        public int maxTicks;
        public int decisionEveryNTicks;

        public List<TapEntry> taps = new List<TapEntry>();   // decision trace, in order

        public string result;               // Win, Lose, Timeout
        public int ticksToResolve;
        public string eventFingerprint;     // recomputed on replay and compared

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
        };

        public static void Write(ReplayLog log, string path)
            => File.WriteAllText(path, JsonConvert.SerializeObject(log, Settings));

        public static ReplayLog Read(string path)
            => JsonConvert.DeserializeObject<ReplayLog>(File.ReadAllText(path));
    }
}