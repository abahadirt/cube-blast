using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Blast.Level
{
    /// <summary>
    /// Level JSON'unu LevelData'ya çevirir. UnityEngine bağımlılığı yoktur:
    /// Unity tarafı TextAsset.text ile, bot harness'ı File.ReadAllText ile
    /// aynı Parse(...) çağrısını kullanır. "Level okuma" tek yerde toplanır.
    /// </summary>
    public static class LevelParser
    {
        private static readonly JsonSerializerSettings Settings = BuildSettings();

        private static JsonSerializerSettings BuildSettings()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new StringEnumConverter()); // "Red" -> CubeColor.Red
            return settings;
        }

        public static LevelData Parse(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentException("Level JSON boş olamaz.", nameof(json));

            LevelData levelData = JsonConvert.DeserializeObject<LevelData>(json, Settings);

            // JSON yukarıdan-aşağı okunur, Core ve presenter tabandan indexler:
            // JSON görüntüsüyle oyunun state'ini eşleştirmek için, JSON'daki satırları ters çeviriyoruz. 
            Array.Reverse(levelData.rows);
            return levelData;
        }
    }
}