using System.Collections.Generic;
using UnityEngine;

namespace CatMouse.Game.Presentation
{
    [DisallowMultipleComponent]
    public sealed class RunVisualEffectPool : MonoBehaviour
    {
        private const int DefaultPoolCapacity = 16;
        private const int EffectSortingOrder = 5;

        [Header("References")]
        [SerializeField] private Transform _effectRoot;
        [SerializeField] private Sprite _pickupSparkleSprite;
        [SerializeField] private Sprite _impactSprite;

        [Header("Pool")]
        [SerializeField, Min(1)] private int _poolCapacity = DefaultPoolCapacity;

        private readonly List<RunVisualEffect> _pool = new();

        private void Awake()
        {
            WarmPool();
        }

        public void PlayPickup(Vector3 position)
        {
            Play(_pickupSparkleSprite, position, 0.72f, 0.22f);
        }

        public void PlayImpact(Vector3 position)
        {
            Play(_impactSprite, position, 0.86f, 0.14f);
        }

        private void OnValidate()
        {
            _poolCapacity = Mathf.Max(1, _poolCapacity);
        }

        private void Play(Sprite sprite, Vector3 position, float scale, float duration)
        {
            if (sprite == null)
            {
                return;
            }

            RunVisualEffect effect = GetAvailableEffect();
            effect?.Spawn(sprite, position, scale, duration, EffectSortingOrder);
        }

        private void WarmPool()
        {
            _effectRoot ??= transform;
            for (int index = _pool.Count; index < _poolCapacity; index++)
            {
                GameObject effectObject = new($"RunVisualEffect_{index:00}");
                effectObject.transform.SetParent(_effectRoot, false);
                RunVisualEffect effect = effectObject.AddComponent<RunVisualEffect>();
                effectObject.SetActive(false);
                _pool.Add(effect);
            }
        }

        private RunVisualEffect GetAvailableEffect()
        {
            for (int index = 0; index < _pool.Count; index++)
            {
                RunVisualEffect effect = _pool[index];
                if (effect != null && effect.IsAvailable)
                {
                    return effect;
                }
            }

            return null;
        }
    }
}
