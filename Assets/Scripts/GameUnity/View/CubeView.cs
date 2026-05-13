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
            // Awake'ten önce çaðrý gelirse fallback
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            _spriteRenderer.color = color;
        }
    }
}