using UnityEngine;

namespace Blast.GameUnity.Level
{
    /// <summary>
    /// Persists the active level index (localStorage in WebGL).
    /// Keeps progress across scene reloads and sessions.
    /// Catalog bounds checking is handled by the flow.
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