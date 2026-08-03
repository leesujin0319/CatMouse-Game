using CatMouse.Game.Run;
using UnityEngine;
using UnityEngine.UI;

namespace CatMouse.Game.UI
{
    [DisallowMultipleComponent]
    public sealed class RunCoinHudView : MonoBehaviour
    {
        [SerializeField] private RunCoinCollector _coinCollector;
        [SerializeField] private Text _coinLabel;

        private void OnEnable()
        {
            if (_coinCollector == null)
            {
                return;
            }

            _coinCollector.CoinCountChanged += SetCoinCount;
            SetCoinCount(_coinCollector.CollectedCoinCount);
        }

        private void OnDisable()
        {
            if (_coinCollector != null)
            {
                _coinCollector.CoinCountChanged -= SetCoinCount;
            }
        }

        private void SetCoinCount(int coinCount)
        {
            if (_coinLabel != null)
            {
                _coinLabel.text = $"{Mathf.Max(0, coinCount):0000} C";
            }
        }
    }
}
