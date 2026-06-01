using Blast.Core.Data;
using System;
using UnityEngine;

namespace Blast.GameUnity.View
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ProjectileView : MonoBehaviour
    {
        [SerializeField] private float _speed = 10f;
        [SerializeField] private CubeColorPalette _palette;
        public CubeColor BallColor { get; private set; }

        private Vector2 _targetPosition;
        private SpriteRenderer _spriteRenderer;

        private Action _onArrived;
        private bool _isMoving;


        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Init(CubeColor color, Vector2 from, Vector2 target, Action onArrivedCallback)
        {
            BallColor = color;
            _spriteRenderer.color = _palette.Get(color);
            transform.position = from;
            _targetPosition = target;
            _onArrived = onArrivedCallback;
            _isMoving = true;
        }


        void Update()
        {
            if (!_isMoving) return;

            transform.position = Vector2.MoveTowards(
                transform.position,
                _targetPosition,
                _speed * Time.deltaTime
            );

            if (Vector2.Distance(transform.position, _targetPosition) < 0.3f)
            {
                transform.position = _targetPosition;
                _isMoving = false;

                var callback = _onArrived;
                _onArrived = null; // prevent double-fire
                callback?.Invoke();
            }
        }

        void OnDisable()
        {
            // clear state when returning to pool
            _onArrived = null;
            _isMoving = false;
        }


    }
}