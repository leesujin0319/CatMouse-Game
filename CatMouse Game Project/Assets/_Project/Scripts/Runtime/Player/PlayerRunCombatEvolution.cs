using CatMouse.Game.Enemy;
using CatMouse.Game.Run;
using UnityEngine;

namespace CatMouse.Game.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerRunCombatEvolution : MonoBehaviour
    {
        private const float LargeProjectileScalePerStack = 0.2f;
        private const float LargeProjectileRadiusPerStack = 0.25f;
        private const float BurstRadius = 2.4f;
        private const int BurstDamage = 3;
        private const float PoisonDuration = 3f;
        private const float CheeseMagnetBaseRadius = 1.6f;
        private const float CheeseMagnetRadiusPerAdditionalStack = 0.8f;
        private const float CheeseMagnetBaseSpeed = 5f;
        private const float CheeseMagnetSpeedPerAdditionalStack = 2f;
        private const float HealthLossReductionPerStack = 0.2f;

        [SerializeField] private PrototypeEnemySpawner _enemySpawner;

        private int _largeProjectileStacks;
        private int _additionalProjectileStacks;
        private int _homingProjectileStacks;
        private int _poisonProjectileStacks;
        private int _cheeseMagnetStacks;
        private int _healthLossReductionStacks;

        public int AdditionalProjectileCount => _additionalProjectileStacks;
        public bool UsesHomingProjectile => _homingProjectileStacks > 0;
        public float ProjectileVisualScale => 1f + (_largeProjectileStacks * LargeProjectileScalePerStack);
        public float ProjectileHitRadiusMultiplier => 1f + (_largeProjectileStacks * LargeProjectileRadiusPerStack);
        public int PoisonDamagePerTick => _poisonProjectileStacks;
        public float PoisonDurationSeconds => _poisonProjectileStacks > 0 ? PoisonDuration : 0f;
        public float CheeseMagnetRadius => _cheeseMagnetStacks > 0
            ? CheeseMagnetBaseRadius + ((_cheeseMagnetStacks - 1) * CheeseMagnetRadiusPerAdditionalStack)
            : 0f;
        public float CheeseMagnetSpeed => _cheeseMagnetStacks > 0
            ? CheeseMagnetBaseSpeed + ((_cheeseMagnetStacks - 1) * CheeseMagnetSpeedPerAdditionalStack)
            : 0f;
        public float HealthLossMultiplier => Mathf.Max(0.4f, 1f - (_healthLossReductionStacks * HealthLossReductionPerStack));

        public void Apply(RunItemDefinition itemDefinition)
        {
            if (itemDefinition == null)
            {
                return;
            }

            switch (itemDefinition.CombatEffectType)
            {
                case RunCombatEffectType.LargeProjectile:
                    _largeProjectileStacks++;
                    break;

                case RunCombatEffectType.AdditionalProjectile:
                    _additionalProjectileStacks++;
                    break;

                case RunCombatEffectType.HomingProjectile:
                    _homingProjectileStacks++;
                    break;

                case RunCombatEffectType.InstantBurst:
                    _enemySpawner?.DamageEnemiesInRadius(transform.position, BurstRadius, BurstDamage);
                    break;

                case RunCombatEffectType.PoisonProjectile:
                    _poisonProjectileStacks++;
                    break;

                case RunCombatEffectType.CheeseMagnet:
                    _cheeseMagnetStacks++;
                    break;

                case RunCombatEffectType.HealthLossReduction:
                    _healthLossReductionStacks++;
                    break;
            }
        }
    }
}
