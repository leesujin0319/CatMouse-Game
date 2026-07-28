using System;
using CatMouse.Game.Player;
using CatMouse.Game.Run;
using UnityEngine;
using UnityEngine.UI;

namespace CatMouse.Game.UI
{
    public sealed class RunItemChoiceCardView : MonoBehaviour
    {
        [SerializeField] private Button _selectButton;
        [SerializeField] private Text _nameLabel;
        [SerializeField] private Text _descriptionLabel;
        [SerializeField] private Text _stackLabel;

        public void Bind(RunItemDefinition itemDefinition, PlayerRunStats runStats, Action<RunItemDefinition> onSelected)
        {
            if (itemDefinition == null || runStats == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (_nameLabel != null)
            {
                _nameLabel.text = itemDefinition.DisplayName;
            }

            if (_descriptionLabel != null)
            {
                _descriptionLabel.text = itemDefinition.Description;
            }

            if (_stackLabel != null)
            {
                _stackLabel.text = $"보유 {runStats.GetStackCount(itemDefinition)} / {itemDefinition.MaximumStacks}";
            }

            if (_selectButton == null)
            {
                return;
            }

            _selectButton.onClick.RemoveAllListeners();
            _selectButton.onClick.AddListener(() => onSelected?.Invoke(itemDefinition));
        }
    }
}
