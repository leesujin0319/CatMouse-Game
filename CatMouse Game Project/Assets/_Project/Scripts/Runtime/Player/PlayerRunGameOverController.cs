using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CatMouse.Game.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerRunGameOverController : MonoBehaviour
    {
        [SerializeField] private PlayerRunHealth _runHealth;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private Button _retryButton;

        private bool _isGameOver;
        private float _timeScaleBeforeGameOver = 1f;

        private void Awake()
        {
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
            Time.timeScale = 0f;
            _gameOverPanel?.SetActive(true);
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
    }
}
