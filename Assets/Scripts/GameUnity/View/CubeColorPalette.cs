using UnityEngine;
using Blast.Core.Data;

namespace Blast.GameUnity.View
{
    [CreateAssetMenu(menuName = "Blast/Cube Color Palette")]
    public class CubeColorPalette : ScriptableObject
    {
        [SerializeField] private Color red = Color.red;
        [SerializeField] private Color blue = Color.blue;
        [SerializeField] private Color green = Color.green;
        [SerializeField] private Color yellow = Color.yellow;
        [SerializeField] private Color black = Color.black;

        public Color Get(CubeColor c) => c switch
        {
            CubeColor.Red => red,
            CubeColor.Blue => blue,
            CubeColor.Green => green,
            CubeColor.Yellow => yellow,
            CubeColor.Black => black,
            _ => Color.white
        };
    }
}