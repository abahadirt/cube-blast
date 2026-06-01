using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Blast.GameUnity.View
{
    public class ShooterView : MonoBehaviour
    {
        [SerializeField] private TextMeshPro ammoText;

        private SpriteRenderer _spriteRenderer;

        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetVisuals(Color color, int ammo)
        {
            SetColor(color);
            SetAmmo(ammo);
        }

        public void SetColor(Color unityColor)
        {
            _spriteRenderer.color = unityColor;
        }

        public void SetAmmo(int ammo)
        {
            if (ammoText != null)
            {
                ammoText.text = ammo.ToString();
            }
        }

        public Tween MoveToPosition(Vector3 targetPosition, float duration)
        {
            return transform.DOMove(targetPosition, duration);
        }


    }

}
