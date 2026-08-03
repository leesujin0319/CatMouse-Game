using System;
using CatMouse.Game.Meta;
using CatMouse.Game.Run;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CatMouse.Game.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerRunGameOverController : MonoBehaviour
    {
        private const int GoalDistanceStepMeters = 100;

        [SerializeField] private PlayerRunHealth _runHealth;
        [SerializeField] private RunProgressController _runProgress;
        [SerializeField] private RunCoinCollector _runCoinCollector;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private Text _distanceValueLabel;
        [SerializeField] private Text _coinValueLabel;
        [SerializeField] private Text _nextGoalValueLabel;
        [SerializeField] private Button _retryButton;

        private bool _isGameOver;
        private bool _hasAppliedCoinReward;
        private float _timeScaleBeforeGameOver = 1f;
        private string _runId;

        private void Awake()
        {
            _runId = Guid.NewGuid().ToString("N");
            _gameOverPanel?.SetActive(false);

            if (_retryButton != null)
            {
                _retryButton.onClick.AddListener(Retry);
            }
        }

        private void OnEnable()
        {
            if (_runHealth != null)
            {
                _runHealth.Depleted += ShowGameOver;
            }
        }

        private void OnDisable()
        {
            if (_runHealth != null)
            {
                _runHealth.Depleted -= ShowGameOver;
            }

            RestoreTimeScale();
        }

        public void Retry()
        {
            if (!_isGameOver)
            {
                return;
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void ShowGameOver()
        {
            if (_isGameOver)
            {
                return;
            }

            _isGameOver = true;
            _timeScaleBeforeGameOver = Time.timeScale;
            ApplyCoinReward();
            RefreshResult();
            Time.timeScale = 0f;
            _gameOverPanel?.transform.SetAsLastSibling();
            _gameOverPanel?.SetActive(true);
        }

        private void ApplyCoinReward()
        {
            if (_hasAppliedCoinReward)
            {
                return;
            }

            _hasAppliedCoinReward = true;
            int collectedCoinCount = GetCollectedCoinCount();
            MetaProgressionService.TryApplyRunResult(new RunResult(_runId, collectedCoinCount));
        }

        private void RefreshResult()
        {
            int distanceMeters = Mathf.FloorToInt(Mathf.Max(0f, _runProgress != null ? _runProgress.DistanceMeters : 0f));
            int nextGoalDistanceMeters = ((distanceMeters / GoalDistanceStepMeters) + 1) * GoalDistanceStepMeters;
            int collectedCoinCount = GetCollectedCoinCount();

            if (_distanceValueLabel != null)
            {
                _distanceValueLabel.text = $"{distanceMeters} m";
            }

            if (_nextGoalValueLabel != null)
            {
                _nextGoalValueLabel.text = $"{nextGoalDistanceMeters} m";
            }

            if (_coinValueLabel != null)
            {
                _coinValueLabel.text = $"{collectedCoinCount}\uAC1C";
            }
        }

        private void RestoreTimeScale()
        {
            if (!_isGameOver)
            {
                return;
            }

            Time.timeScale = _timeScaleBeforeGameOver;
            _isGameOver = false;
        }

        private int GetCollectedCoinCount()
        {
            _runCoinCollector ??= FindFirstObjectByType<RunCoinCollector>();
            return _runCoinCollector != null ? _runCoinCollector.CollectedCoinCount : 0;
        }
    }
}
