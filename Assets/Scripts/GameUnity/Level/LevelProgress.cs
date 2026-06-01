using UnityEngine;

namespace Blast.GameUnity.Level
{
    /// <summary>
    /// Aktif level index'ini kalıcılaştırır (WebGL'de localStorage).
    /// Sahne reload'ları ve oturumlar arası ilerleme korunur.
    /// Katalog sınır kontrolü burada DEĞİL, akışta yapılır.
    /// </summary>
    public static class LevelProgress
    {
        private const string Key = "blast.current_level";

        public static int CurrentIndex
        {
            get => PlayerPrefs.GetInt(Key, 0);
            set
            {
                PlayerPrefs.SetInt(Key, Mathf.Max(0, value));
                PlayerPrefs.Save();
            }
        }

        public static void Reset() => CurrentIndex = 0;
    }
}