using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Blast.Level
{
    /// <summary>
    /// Parses level JSON into LevelData. No UnityEngine dependency.
    /// Used by both Unity and bot harness for consistent level loading.
    /// </summary>
    public static class LevelParser
    {
        private static readonly JsonSerializerSettings Settings = BuildSettings();

        private static JsonSerializerSettings BuildSettings()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new StringEnumConverter()); // Converts string to enum (e.g. "Red" -> CubeColor.Red)
            return settings;
        }

        public static LevelData Parse(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentException("Level JSON cannot be null or empty.", nameof(json));

            LevelData levelData = JsonConvert.DeserializeObject<LevelData>(json, Settings);

            // Reverse rows to match bottom-up indexing used in Core and presenter.
            Array.Reverse(levelData.rows);
            return levelData;
        }
    }
}