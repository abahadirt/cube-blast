using UnityEngine;

namespace Blast.GameUnity.Level
{
    /// <summary>
    /// Aktif level index'ini kalýcýlaþtýrýr (WebGL'de localStorage).
    /// Sahne reload'larý ve oturumlar arasý ilerleme korunur.
    /// Katalog sýnýr kontrolü burada DEÐÝL, akýþta yapýlýr.
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