namespace CatMouse.Game.Player
{
    public readonly struct PlayerStatsSnapshot
    {
        public PlayerStatsSnapshot(
            int attackDamage,
            float attacksPerSecond,
            float attackRange,
            float projectileSpeed,
            float forwardSpeed,
            float verticalSpeed)
        {
            AttackDamage = attackDamage;
            AttacksPerSecond = attacksPerSecond;
            AttackRange = attackRange;
            ProjectileSpeed = projectileSpeed;
            ForwardSpeed = forwardSpeed;
            VerticalSpeed = verticalSpeed;
        }

        public int AttackDamage { get; }
        public float AttacksPerSecond { get; }
        public float AttackRange { get; }
        public float ProjectileSpeed { get; }
        public float ForwardSpeed { get; }
        public float VerticalSpeed { get; }
    }
}
