namespace Net.SnakeGame
{
    public class Apple
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Apple() =>
            CreateApple();

        public void CreateApple()
        {
            X = Game.Random.Next(Game.Width);
            Y = Game.Random.Next(Game.Height);
        }
    }
}
