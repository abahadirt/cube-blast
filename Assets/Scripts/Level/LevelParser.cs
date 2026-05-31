using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Blast.Level
{
    /// <summary>
    /// Level JSON'unu LevelData'ya çevirir. UnityEngine baðýmlýlýðý yoktur:
    /// Unity tarafý TextAsset.text ile, bot harness'ý File.ReadAllText ile
    /// ayný Parse(...) çaðrýsýný kullanýr. "Level okuma" tek yerde toplanýr.
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
                throw new ArgumentException("Level JSON boþ olamaz.", nameof(json));

            LevelData levelData = JsonConvert.DeserializeObject<LevelData>(json, Settings);

            // JSON yukarýdan-aþaðý okunur, Core ve presenter tabandan indexler:
            // JSON görüntüsüyle oyunun state'ini eþleþtirmek için, JSON'daki satýrlarý ters çeviriyoruz. 
            Array.Reverse(levelData.rows);
            return levelData;
        }
    }
}