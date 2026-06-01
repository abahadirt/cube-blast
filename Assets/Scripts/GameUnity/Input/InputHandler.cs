using UnityEngine;
using UnityEngine.InputSystem;
using Blast.GameUnity.View;

using System;
namespace Blast.GameUnity.Input
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private ShooterReserveView _reserveView;
        public event Action<int> OnColumnTapped;
        void Update()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2? screenPos = GetTapPosition();
                if (!screenPos.HasValue) return;

                Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos.Value);
                int col = _reserveView.GetColumnIndexFromWorldX(worldPos.x);

                if (col >= 0) OnColumnTapped?.Invoke(col);
            }
        }

        private Vector2? GetTapPosition()
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                return Touchscreen.current.primaryTouch.position.ReadValue();
            }
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                return Mouse.current.position.ReadValue();
            }
            return null;
        }
    }
}