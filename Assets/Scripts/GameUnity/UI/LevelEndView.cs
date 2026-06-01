using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Blast.GameUnity.UI
{
    /// <summary>
    /// End-of-level overlay. Not part of the simulation, so it has no Contract port;
    /// app-flow (GameFlowController) drives it directly. Flow decides WHEN a level is
    /// resolved and WHAT happens on the button; this view owns HOW it looks and how
    /// long the outcome takes to appear .
    /// </summary>
    public class LevelEndView : MonoBehaviour
    {

        [Header("Refs")]
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _buttonText;
        [SerializeField] private Button _actionButton;

        [Header("Timing")]
        [Tooltip("Beat before the panel appears, so the final shot resolves and the player registers the outcome.")]
        [SerializeField] private float _settleDelay = 0.8f;
        [SerializeField] private float _appearDuration = 0.5f;

        private Action _onAction;

        private Tween _showTween;


        private void Awake()
        {
            _actionButton.onClick.AddListener(HandleClick);
            Hide();
        }

        private void OnDestroy()
        {
            _actionButton.onClick.RemoveListener(HandleClick);
            _showTween?.Kill();
        }

        public void ShowWin(Action onContinue) => Show("Seviye Tamamlandı!", "Sonraki Seviye", onContinue);
        public void ShowLose(Action onRetry) => Show("Slotlar Doldu!", "Tekrar Dene", onRetry);

        private void Show(string title, string buttonLabel, Action onAction)
        {
            _onAction = onAction;
            _titleText.text = title;
            _buttonText.text = buttonLabel;

            // 1. Önceki animasyon veya bekleme süreci varsa İPTAL ET (StopCoroutine'in karşılığı)
            _showTween?.Kill();

            // 2. Paneli hazırlayıp sıfırla
            _root.SetActive(false);
            _root.transform.localScale = Vector3.zero;

            // 3. DOTween Sequence (Sıralı İşlem) başlat
            _showTween = DOTween.Sequence()
                .AppendInterval(_settleDelay)
                .AppendCallback(() => _root.SetActive(true))
                .Append(_root.transform.DOScale(Vector3.one, _appearDuration).SetEase(Ease.OutBack))
                .SetUpdate(true);

        }

        private void Hide() => _root.SetActive(false);

        private void HandleClick()
        {
            Hide();
            var callback = _onAction;
            _onAction = null;     // double-fire koruması
            callback?.Invoke();
        }
    }
}