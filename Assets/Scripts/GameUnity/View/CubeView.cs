using UnityEngine;

namespace Blast.GameUnity.View
{

    public class CubeView : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        public void Init(Color color)
        {
            SetColor(color);
        }
        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetColor(Color color)
        {
            // Fallback in case this is called before Awake
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            _spriteRenderer.color = color;
        }
    }
}