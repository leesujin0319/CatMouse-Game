namespace CatMouse.Game.Run
{
    public readonly struct RunResult
    {
        public RunResult(string runId, int coinReward)
        {
            RunId = runId;
            CoinReward = coinReward;
        }

        public string RunId { get; }
        public int CoinReward { get; }
        public int CheeseReward => CoinReward;
    }
}
