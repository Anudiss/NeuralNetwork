using System.Diagnostics;

namespace Net.SnakeGame
{
    public class Snake
    {
        private Direction _direction = Direction.Up;
        private Direction _lastDirection = Direction.Up;
        private int _score = 0;
        private bool _isScoreChanged = false;
        private bool isAlive = true;

        public NeuralNet Net { get; set; }

        public Queue<Tail> Tails { get; set; } = new();
        public int SnakeLength => Tails.Count;

        public Stopwatch Stopwatch { get; set; } = new();

        public Tail Head
        {
            get
            {
                lock (Tails)
                {
                    return Tails.Last();
                }
            }
        }

        public Direction Direction
        {
            get => _direction;
            set
            {
                if (Math.Abs(((int)_lastDirection) % 2) == Math.Abs(((int)value) % 2))
                    return;

                _direction = value;
            }
        }

        public bool IsAlive
        {
            get => isAlive; 
            private set
            {
                if (value == false)
                {
                    Stopwatch.Stop();
                }

                isAlive = value;
            }
        }
        public int Score
        {
            get => _score;
            set
            {
                if (_score != value)
                    _isScoreChanged = true;
                _score = value;
            }
        }

        public Snake(int initialLength)
        {
            InitializeNetwork();
        }

        private void InitializeNetwork()
        {
            Net = new();
            Net.Initialize(1, inputNeuronCount: 4,
                              hiddenNeuronCount: 4,
                              outputNeuronCount: 4);
        }

        public void InitializeSnake(int initialLength)
        {
            IsAlive = true;
            _direction = Direction.Up;
            if (Tails.Any())
                Tails.Clear();

            var initialX = Game.Width / 2;
            var initialY = (Game.Height - initialLength) / 2;

            for (int i = 0; i < initialLength; i++)
                Tails.Enqueue(new Tail()
                {
                    X = initialX,
                    Y = initialY - i
                });

            Stopwatch.Reset();
        }

        public void Move()
        {
            if (!Stopwatch.IsRunning)
                Stopwatch.Start();

            lock (Tails)
            {
                if (!IsAlive)
                    return;

                Tail newTail;
                if (_isScoreChanged)
                {
                    newTail = new();
                    _isScoreChanged = false;
                }
                else
                    newTail = Tails.Dequeue();

                newTail.X = Head.X + (((int)Direction) % 2);
                newTail.Y = Head.Y + (((int)Direction + 1) % 2);

                Console.Title = $"Moving to: {{{newTail.X}, {newTail.Y}}} | Score: {Score}";

                if (Tails.Any(tail => tail.X == newTail.X && tail.Y == newTail.Y))
                    IsAlive = false;


                if (newTail.X <= 0 || newTail.X >= Game.Width - 1 || newTail.Y <= 0 || newTail.Y >= Game.Height - 1)
                    IsAlive = false;

                Tails.Enqueue(newTail);

                _lastDirection = Direction;
            }
        }

        public (int score, long liveTime) GetDeathInfo() => (Score, Stopwatch.ElapsedMilliseconds);

        public void EatApple(Apple apple)
        {
            if (!IsAlive)
                return;

            if (Head.X == apple.X && Head.Y == apple.Y)
            {
                apple.CreateApple();
                Score++;
            }
        }

        public void MoveUp() => Direction = Direction.Up;
        public void MoveDown() => Direction = Direction.Down;
        public void MoveRight() => Direction = Direction.Right;
        public void MoveLeft() => Direction = Direction.Left;

        public void NetworkMoving()
        {
            double max = Math.Max(Math.Max(Net.OutputLayer[0].Output, Net.OutputLayer[1].Output),
                                  Math.Max(Net.OutputLayer[2].Output, Net.OutputLayer[3].Output));

            if (Net.OutputLayer[0].Output == max)
                MoveLeft();
            else if (Net.OutputLayer[1].Output == max)
                MoveUp();
            else if (Net.OutputLayer[2].Output == max)
                MoveRight();
            else if (Net.OutputLayer[3].Output == max)
                MoveDown();
        }
    }

    public class Tail
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public enum Direction
    {
        Left = -1,
        Up = -2,
        Right = 1,
        Down = 0
    }
}
