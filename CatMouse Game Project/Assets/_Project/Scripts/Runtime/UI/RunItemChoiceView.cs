using System;
using System.Collections.Generic;
using CatMouse.Game.Player;
using CatMouse.Game.Run;
using UnityEngine;

namespace CatMouse.Game.UI
{
    [DisallowMultipleComponent]
    public sealed class RunItemChoiceView : MonoBehaviour
    {
        [SerializeField] private RunItemChoiceCardView[] _cards = Array.Empty<RunItemChoiceCardView>();

        public int CardCount => _cards?.Length ?? 0;

        public void Show(
            IReadOnlyList<RunItemDefinition> choices,
            PlayerRunStats runStats,
            Action<RunItemDefinition> onSelected)
        {
            if (choices == null || choices.Count != CardCount)
            {
                return;
            }

            gameObject.SetActive(true);

            for (int index = 0; index < _cards.Length; index++)
            {
                _cards[index]?.Bind(choices[index], runStats, onSelected);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
