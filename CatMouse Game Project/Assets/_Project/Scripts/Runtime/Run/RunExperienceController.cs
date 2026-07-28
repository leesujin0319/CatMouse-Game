using System;
using UnityEngine;

namespace CatMouse.Game.Run
{
    [DisallowMultipleComponent]
    public sealed class RunExperienceController : MonoBehaviour
    {
        private const int DefaultInitialLevel = 1;
        private const float DefaultExperiencePerLevel = 10f;

        [Header("References")]
        [SerializeField] private RunItemChoiceController _itemChoiceController;

        [Header("Progress")]
        [SerializeField, Min(1)] private int _initialLevel = DefaultInitialLevel;
        [SerializeField, Min(0.01f)] private float _experiencePerLevel = DefaultExperiencePerLevel;

        private int _level;
        private int _pendingChoices;
        private float _currentExperience;

        public event Action<int, float, float> ExperienceChanged;

        public int Level => _level;
        public float CurrentExperience => _currentExperience;
        public float ExperienceToNextLevel => _experiencePerLevel;

        public void Configure(RunItemChoiceController itemChoiceController)
        {
            _itemChoiceController = itemChoiceController;
        }

        private void Awake()
        {
            _level = _initialLevel;
            NotifyExperienceChanged();
        }

        private void Update()
        {
            TryRequestPendingChoice();
        }

        private void OnValidate()
        {
            _initialLevel = Mathf.Max(1, _initialLevel);
            _experiencePerLevel = Mathf.Max(0.01f, _experiencePerLevel);
        }

        public void GainExperience(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            float remainingExperience = amount;
            while (remainingExperience > 0f)
            {
                float experienceNeeded = _experiencePerLevel - _currentExperience;
                if (remainingExperience < experienceNeeded)
                {
                    _currentExperience += remainingExperience;
                    break;
                }

                remainingExperience -= experienceNeeded;
                _currentExperience = 0f;
                _level++;
                _pendingChoices++;
            }

            NotifyExperienceChanged();
            TryRequestPendingChoice();
        }

        private void TryRequestPendingChoice()
        {
            if (!Application.isPlaying ||
                _pendingChoices <= 0 ||
                _itemChoiceController == null ||
                _itemChoiceController.IsChoosing)
            {
                return;
            }

            _itemChoiceController.RequestChoice();
            if (_itemChoiceController.IsChoosing)
            {
                _pendingChoices--;
            }
        }

        private void NotifyExperienceChanged()
        {
            ExperienceChanged?.Invoke(_level, _currentExperience, _experiencePerLevel);
        }
    }
}
