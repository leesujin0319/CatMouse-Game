using CatMouse.Game.Player;
using UnityEngine;
using UnityEngine.UI;

namespace CatMouse.Game.UI
{
    [DisallowMultipleComponent]
    public sealed class PlayerRunHealthBarView : MonoBehaviour
    {
        [SerializeField] private PlayerRunHealth _runHealth;
        [SerializeField] private Image _fillImage;
        [SerializeField] private Text _valueLabel;

        private void OnEnable()
        {
            if (_runHealth != null)
            {
                _runHealth.HealthChanged += Refresh;
            }

            RefreshCurrentValue();
        }

        private void Start()
        {
            RefreshCurrentValue();
        }

        private void OnDisable()
        {
            if (_runHealth != null)
            {
                _runHealth.HealthChanged -= Refresh;
            }
        }

        private void Refresh(float currentHealth, float maximumHealth)
        {
            if (_fillImage != null)
            {
                float normalizedHealth = maximumHealth > 0f ? currentHealth / maximumHealth : 0f;
                _fillImage.fillAmount = normalizedHealth;

                RectTransform fillRect = _fillImage.rectTransform;
                Vector2 anchorMax = fillRect.anchorMax;
                anchorMax.x = normalizedHealth;
                fillRect.anchorMax = anchorMax;
            }

            if (_valueLabel != null)
            {
                _valueLabel.text = $"HP {Mathf.FloorToInt(currentHealth)} / {Mathf.FloorToInt(maximumHealth)}";
            }
        }

        private void RefreshCurrentValue()
        {
            if (_runHealth != null)
            {
                Refresh(_runHealth.CurrentHealth, _runHealth.MaximumHealth);
            }
        }
    }
}
