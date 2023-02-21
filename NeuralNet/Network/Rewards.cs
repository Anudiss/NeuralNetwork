namespace NeuralNet.Network
{
    public static class Rewards
    {
        public static readonly double GoToApple = 5;
        public static readonly double GoFromApple = -5;
        public static readonly double DieByWall = -200;
        public static readonly double EatHimself = -200;
        public static readonly double EatApple = 300;
        public static readonly double Live = 30;
    }
}
