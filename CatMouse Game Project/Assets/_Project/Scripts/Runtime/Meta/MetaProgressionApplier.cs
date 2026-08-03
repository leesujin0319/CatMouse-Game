using CatMouse.Game.Player;
using UnityEngine;

namespace CatMouse.Game.Meta
{
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public sealed class MetaProgressionApplier : MonoBehaviour
    {
        [SerializeField] private PlayerRunStats _runStats;
        [SerializeField] private MetaProgressionCatalog _catalog;

        private void Start()
        {
            ApplyProgression();
        }

        public void ApplyProgression()
        {
            if (_runStats == null || _catalog == null)
            {
                return;
            }

            MetaProgressionService.Initialize(_catalog);
            _runStats.ClearPersistentItems();
            MetaProgressionLoadout loadout = MetaProgressionService.GetLoadout();
            foreach (MetaRunItemStack itemStack in loadout.PersistentItemStacks)
            {
                _runStats.SetPersistentStackCount(itemStack.ItemDefinition, itemStack.StackCount);
            }
        }
    }
}
