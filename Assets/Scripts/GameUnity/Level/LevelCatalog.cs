using UnityEngine;

namespace Blast.GameUnity.Level
{
    [CreateAssetMenu(menuName = "Blast/Level Catalog", fileName = "LevelCatalog")]
    public class LevelCatalog : ScriptableObject
    {
        [Tooltip("Leveller bu sıraya göre oynanır.")]
        [SerializeField] private TextAsset[] _levels;

        public int Count => _levels.Length;
        public TextAsset Get(int index) => _levels[index];
        public bool IsValidIndex(int index) => index >= 0 && index < _levels.Length;

    }
}