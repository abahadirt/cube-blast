using System;
using System.Collections.Generic;
using System.IO;
using Blast.Level;

namespace Blast.Bot.Cli
{
    /// <summary>
    /// Loads level data for the bot CLI.
    /// </summary>
    public sealed class LevelRepository
    {
        public string LoadJson(string id)
        {
            if (File.Exists(id)) return File.ReadAllText(id);
            string file = id.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ? id : id + ".json";
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                string candidate = Path.Combine(dir.FullName, "Assets", "Levels", file);
                if (File.Exists(candidate)) return File.ReadAllText(candidate);
                dir = dir.Parent;
            }
            return null;
        }

        public LevelData Load(string id)
        {
            string json = LoadJson(id) ?? throw new ArgumentException($"Level not found: '{id}'.");
            return LevelParser.Parse(json);
        }

        public IReadOnlyList<string> EnumerateIds()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                string levelsDir = Path.Combine(dir.FullName, "Assets", "Levels");
                if (Directory.Exists(levelsDir))
                {
                    var ids = new List<string>();
                    foreach (string f in Directory.GetFiles(levelsDir, "*.json"))
                        if (f.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                            ids.Add(Path.GetFileNameWithoutExtension(f));
                    ids.Sort(StringComparer.Ordinal);
                    return ids;
                }
                dir = dir.Parent;
            }
            return new List<string>();
        }
    }
}