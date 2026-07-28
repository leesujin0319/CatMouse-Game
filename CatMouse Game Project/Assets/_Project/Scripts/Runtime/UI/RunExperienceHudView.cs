using CatMouse.Game.Run;
using UnityEngine;
using UnityEngine.UI;

namespace CatMouse.Game.UI
{
    [DisallowMultipleComponent]
    public sealed class RunExperienceHudView : MonoBehaviour
    {
        [SerializeField] private RunExperienceController _experience;
        [SerializeField] private Image _fillImage;
        [SerializeField] private Text _valueLabel;

        private void OnEnable()
        {
            if (_experience != null)
            {
                _experience.ExperienceChanged += Refresh;
                RefreshCurrentValue();
            }
        }

        private void OnDisable()
        {
            if (_experience != null)
            {
                _experience.ExperienceChanged -= Refresh;
            }
        }

        private void Refresh(int level, float currentExperience, float experienceToNextLevel)
        {
            float normalizedExperience = experienceToNextLevel > 0f
                ? currentExperience / experienceToNextLevel
                : 0f;
            if (_fillImage != null)
            {
                _fillImage.fillAmount = normalizedExperience;

                RectTransform fillRect = _fillImage.rectTransform;
                Vector2 anchorMax = fillRect.anchorMax;
                anchorMax.x = normalizedExperience;
                fillRect.anchorMax = anchorMax;
            }

            if (_valueLabel != null)
            {
                _valueLabel.text = $"Lv. {level}  XP {Mathf.FloorToInt(currentExperience)} / {Mathf.FloorToInt(experienceToNextLevel)}";
            }
        }

        private void RefreshCurrentValue()
        {
            if (_experience != null)
            {
                Refresh(
                    _experience.Level,
                    _experience.CurrentExperience,
                    _experience.ExperienceToNextLevel);
            }
        }
    }
}
