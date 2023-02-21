using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Net.SnakeGame
{
    public class Game
    {
        private static Random _random = default!;
        public static Random Random => _random ??= new Random();

        public static bool Visualize = false;

        public const int Width = 40;
        public const int Height = 20;

        public Snake Snake;
        public Apple Apple;

        public Timer timer;

        public Game()
        {
            Snake = new(4);

            InitalizeGame();
        }

        public void InitalizeGame()
        {
            Snake.InitializeSnake(4);
            Apple = new();

            timer = new(200)
            {
                AutoReset = true
            };
            timer.Elapsed += ElapsedTick;
        }

        public void PulseNetwork()
        {
        }

        public void Start()
        {
            timer.Start();
        }

        private void Tick()
        {
            Snake.Move();
            Snake.EatApple(Apple);

            if (Visualize)
                Draw();

            if (Snake.IsAlive == false)
            {
                Dead();
            }
        }

        private void Dead()
        {
            Console.Title = "Dead";

            timer.Stop();
            timer.Elapsed -= ElapsedTick;
        }

        private void ElapsedTick(object? sender, ElapsedEventArgs args) => Tick();

        private void Draw()
        {
            Console.Clear();

            foreach (var tail in Snake.Tails)
            {
                Console.SetCursorPosition(tail.X, tail.Y);
                Console.Write("@");
            }

            Console.SetCursorPosition(Apple.X, Apple.Y);
            Console.Write("A");
/*
            Console.SetCursorPosition(Width + 2, 2);
            Console.Write($"Left:   {Program.Net.OutputLayer[0].Output}");

            Console.SetCursorPosition(Width + 2, 3);
            Console.Write($"Up:     {Program.Net.OutputLayer[1].Output}");

            Console.SetCursorPosition(Width + 2, 4);
            Console.Write($"Right:  {Program.Net.OutputLayer[2].Output}");

            Console.SetCursorPosition(Width + 2, 5);
            Console.Write($"Down:   {Program.Net.OutputLayer[3].Output}");*/
        }

        /*private static void PulseNetwork()
        {
            double max = Math.Max(Math.Max(Program.Net.OutputLayer[0].Output, Program.Net.OutputLayer[1].Output), 
                                  Math.Max(Program.Net.OutputLayer[2].Output, Program.Net.OutputLayer[3].Output));

            if (Program.Net.OutputLayer[0].Output == max)
                Instance.Snake.MoveLeft();
            else if (Program.Net.OutputLayer[1].Output == max)
                Instance.Snake.MoveUp();
            else if (Program.Net.OutputLayer[2].Output == max)
                Instance.Snake.MoveRight();
            else if (Program.Net.OutputLayer[3].Output == max)
                Instance.Snake.MoveDown();
        }*/
    }
}
