using System;
using System.Collections.Generic;

namespace Blast.Bot.Cli
{
    /// <summary>
    /// Parsed command-line options for the bot CLI.
    /// </summary>
    public sealed class CliOptions
    {
        public string LevelArg { get; }     // positional level id/path, or null if the first arg is a flag
        public string Levels { get; }       // --levels <ids|all>
        public string Policy { get; }       // --policy <name>          (default "stochastic")
        public string Policies { get; }     // --policies <names|all>
        public string Mode { get; }         // --mode fair|oracle       (default "oracle")
        public int SeedStart { get; }       // --seed N                 (default 0)
        public int SeedCount { get; }       // --seeds N                (default 1)
        public int Repeat { get; }          // --repeat R               (default 1)
        public string CsvDir { get; }       // --csv DIR
        public string RecordPath { get; }   // --record path.json

        private CliOptions(string levelArg, Dictionary<string, string> flags)
        {
            LevelArg = levelArg;
            Levels = Value(flags, "levels");
            Policy = Value(flags, "policy") ?? "stochastic";
            Policies = Value(flags, "policies");
            Mode = Value(flags, "mode") ?? "oracle";
            SeedStart = Int(flags, "seed", 0);
            SeedCount = Int(flags, "seeds", 1);
            Repeat = Int(flags, "repeat", 1);
            CsvDir = Value(flags, "csv");
            RecordPath = Value(flags, "record");
        }

        public static CliOptions Parse(string[] args)
        {
            bool firstIsFlag = args.Length == 0 || args[0].StartsWith("--");
            string levelArg = firstIsFlag ? null : args[0];
            return new CliOptions(levelArg, ParseFlags(args, firstIsFlag ? 0 : 1));
        }

        private static Dictionary<string, string> ParseFlags(string[] args, int start)
        {
            var flags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = start; i < args.Length; i++)
            {
                if (!args[i].StartsWith("--")) continue;
                string key = args[i].Substring(2);
                flags[key] = (i + 1 < args.Length && !args[i + 1].StartsWith("--")) ? args[++i] : "true";
            }
            return flags;
        }

        private static string Value(Dictionary<string, string> flags, string key)
            => flags.TryGetValue(key, out var v) ? v : null;

        private static int Int(Dictionary<string, string> flags, string key, int fallback)
        {
            if (!flags.TryGetValue(key, out var v)) return fallback;
            if (int.TryParse(v, out var n)) return n;
            throw new ArgumentException($"--{key} expects an integer, got '{v}'.");
        }
    }
}