using System;
using System.Collections.Generic;
using CatMouse.Game.Player;
using CatMouse.Game.UI;
using UnityEngine;

namespace CatMouse.Game.Run
{
    [DisallowMultipleComponent]
    public sealed class RunItemChoiceController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerRunStats _runStats;
        [SerializeField] private RunItemChoiceView _choiceView;
        [SerializeField] private PlayerScreenPositionController _screenPositionController;
        [SerializeField] private RunItemDefinition[] _availableItems = Array.Empty<RunItemDefinition>();

        private readonly List<RunItemDefinition> _activeChoices = new();

        private float _timeScaleBeforeChoice = 1f;
        private bool _isChoosing;

        public bool IsChoosing => _isChoosing;

        private void Awake()
        {
            _choiceView?.Hide();
        }

        private void OnDisable()
        {
            EndChoice();
        }

        private void OnValidate()
        {
            _availableItems ??= Array.Empty<RunItemDefinition>();
        }

        public void RequestChoice()
        {
            if (_isChoosing || !HasValidConfiguration())
            {
                return;
            }

            CollectChoices();
            if (_activeChoices.Count < _choiceView.CardCount)
            {
                return;
            }

            _isChoosing = true;
            _timeScaleBeforeChoice = Time.timeScale;
            Time.timeScale = 0f;
            _choiceView.Show(_activeChoices, _runStats, ChooseItem);
        }

        private void ChooseItem(RunItemDefinition itemDefinition)
        {
            if (!_isChoosing || !_activeChoices.Contains(itemDefinition))
            {
                return;
            }

            bool applied = itemDefinition.IsTemporary
                ? _screenPositionController != null && _screenPositionController.TryStartTemporarySpeedEffect(itemDefinition)
                : _runStats.TryAcquire(itemDefinition);
            if (!applied)
            {
                return;
            }

            EndChoice();
        }

        private void CollectChoices()
        {
            _activeChoices.Clear();

            List<RunItemDefinition> candidates = new();
            for (int index = 0; index < _availableItems.Length; index++)
            {
                RunItemDefinition itemDefinition = _availableItems[index];
                if (itemDefinition != null
                    && _runStats.GetStackCount(itemDefinition) < itemDefinition.MaximumStacks
                    && !candidates.Contains(itemDefinition))
                {
                    candidates.Add(itemDefinition);
                }
            }

            int choiceCount = Mathf.Min(_choiceView.CardCount, candidates.Count);
            for (int index = 0; index < choiceCount; index++)
            {
                int candidateIndex = UnityEngine.Random.Range(0, candidates.Count);
                _activeChoices.Add(candidates[candidateIndex]);
                candidates.RemoveAt(candidateIndex);
            }
        }

        private void EndChoice()
        {
            if (_choiceView != null)
            {
                _choiceView.Hide();
            }

            if (!_isChoosing)
            {
                return;
            }

            Time.timeScale = _timeScaleBeforeChoice;
            _isChoosing = false;
            _activeChoices.Clear();
        }

        private bool HasValidConfiguration()
        {
            return _runStats != null
                && _runStats.IsInitialized
                && _choiceView != null
                && _choiceView.CardCount > 0
                && _availableItems.Length >= _choiceView.CardCount;
        }
    }
}
