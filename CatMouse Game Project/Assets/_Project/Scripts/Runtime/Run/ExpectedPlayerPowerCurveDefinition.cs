using System;
using UnityEngine;

namespace CatMouse.Game.Run
{
    [Serializable]
    public struct ExpectedPlayerPowerMilestone
    {
        [SerializeField, Min(1)] private int _difficultyLevel;
        [SerializeField, Min(0.01f)] private float _expectedSingleTargetDps;

        public int DifficultyLevel => _difficultyLevel;
        public float ExpectedSingleTargetDps => _expectedSingleTargetDps;

        public void Validate()
        {
            _difficultyLevel = Mathf.Max(1, _difficultyLevel);
            _expectedSingleTargetDps = Mathf.Max(0.01f, _expectedSingleTargetDps);
        }
    }

    [CreateAssetMenu(
        fileName = "ExpectedPlayerPowerCurve",
        menuName = "CatMouse/Run/Expected Player Power Curve")]
    public sealed class ExpectedPlayerPowerCurveDefinition : ScriptableObject
    {
        [Header("Expected Single Target DPS")]
        [SerializeField] private ExpectedPlayerPowerMilestone[] _milestones = Array.Empty<ExpectedPlayerPowerMilestone>();
        [SerializeField, Min(0f)] private float _dpsIncreasePerLevelAfterLastMilestone = 1f;

        public float GetExpectedSingleTargetDps(int difficultyLevel)
        {
            if (_milestones == null || _milestones.Length == 0)
            {
                return 1f;
            }

            int targetLevel = Mathf.Max(1, difficultyLevel);
            int lowerLevel = int.MinValue;
            float lowerDps = 0f;
            int upperLevel = int.MaxValue;
            float upperDps = 0f;

            for (int index = 0; index < _milestones.Length; index++)
            {
                ExpectedPlayerPowerMilestone milestone = _milestones[index];
                if (milestone.DifficultyLevel <= targetLevel
                    && milestone.DifficultyLevel >= lowerLevel)
                {
                    lowerLevel = milestone.DifficultyLevel;
                    lowerDps = milestone.ExpectedSingleTargetDps;
                }

                if (milestone.DifficultyLevel > targetLevel
                    && milestone.DifficultyLevel < upperLevel)
                {
                    upperLevel = milestone.DifficultyLevel;
                    upperDps = milestone.ExpectedSingleTargetDps;
                }
            }

            if (lowerLevel == int.MinValue)
            {
                return upperDps;
            }

            if (upperLevel == int.MaxValue)
            {
                return lowerDps +
                    ((targetLevel - lowerLevel) * _dpsIncreasePerLevelAfterLastMilestone);
            }

            float progress = Mathf.InverseLerp(lowerLevel, upperLevel, targetLevel);
            return Mathf.Lerp(lowerDps, upperDps, progress);
        }

        public float GetExpectedDpsMultiplier(int difficultyLevel)
        {
            float baseDps = GetExpectedSingleTargetDps(1);
            if (baseDps <= Mathf.Epsilon)
            {
                return 1f;
            }

            return Mathf.Max(1f, GetExpectedSingleTargetDps(difficultyLevel) / baseDps);
        }

        private void OnValidate()
        {
            _dpsIncreasePerLevelAfterLastMilestone = Mathf.Max(
                0f,
                _dpsIncreasePerLevelAfterLastMilestone);

            if (_milestones == null)
            {
                _milestones = Array.Empty<ExpectedPlayerPowerMilestone>();
                return;
            }

            for (int index = 0; index < _milestones.Length; index++)
            {
                ExpectedPlayerPowerMilestone milestone = _milestones[index];
                milestone.Validate();
                _milestones[index] = milestone;
            }
        }
    }
}
