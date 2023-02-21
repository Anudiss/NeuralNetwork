using NeuralNet.Network;

namespace NeuralNet.GameLogic
{
    public class Snake
    {
        private Direction _direction = Direction.Up;
        private Direction _lastDirection = Direction.Up;
        private bool _shouldAdd = false;

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
        public Direction LastDirection => _lastDirection;

        public Queue<Tail> Tails { get; set; }
        public Tail Head => Tails.Last();

        public double Score { get; set; } = 0;

        public bool IsAlive { get; set; } = true;

        private Field _field;

        public Snake(Field field, int initialLength = Game.InitialSnakeLength)
        {
            _field = field;
            Tails = new();

            InitialSnake(initialLength);
        }

        public void InitialSnake(int initialLength = Game.InitialSnakeLength)
        {
            int halfWidth = Game.FieldWidth / 2;
            int halfHeight = (Game.FieldHeight - initialLength) / 2;

            for (int i = 0; i < initialLength; i++)
                Tails.Enqueue(new()
                {
                    X = halfWidth,
                    Y = halfHeight - i
                });
        }

        public void Move()
        {
            if (!IsAlive)
                return;

            Tail newTail;
            if (_shouldAdd)
            {
                newTail = new();
                _shouldAdd = false;
            }
            else
                newTail = Tails.Dequeue();

            newTail.X = Head.X + (((int)Direction) % 2);
            newTail.Y = Head.Y + (((int)Direction + 1) % 2);

            if (Tails.Any(tail => tail.Intersect(newTail)))
            {
                Score += Rewards.EatHimself;
                IsAlive = false;
            }
            else if (newTail.X < 0 || newTail.X > Game.FieldWidth - 1 ||
                     newTail.Y < 0 || newTail.Y > Game.FieldHeight - 1)
            {
                Score += Rewards.DieByWall;
                IsAlive = false;
            }
            else
                Score += Rewards.Live;

            Tails.Enqueue(newTail);

            _lastDirection = Direction;
        }

        public double GetReward(Direction direction)
        {
            Tail tail = new()
            {
                X = Head.X + (((int)direction) % 2),
                Y = Head.Y + (((int)direction + 1) % 2)
            };

            if (Tails.Any(_tail => tail.Intersect(_tail)))
                return Rewards.EatHimself;

            if (tail.X < 0 || tail.X > Game.FieldWidth - 1 ||
                tail.Y < 0 || tail.Y > Game.FieldHeight - 1)
                return Rewards.DieByWall;

            if (tail.Intersect(_field.Apple))
                return Rewards.EatApple;

            if (Math.Sqrt(Math.Pow(_field.Apple.X - tail.X, 2) + Math.Pow(_field.Apple.Y - tail.Y, 2)) < Dictance(_field.Apple))
                return Rewards.GoToApple;
            else
                return Rewards.GoFromApple;
        }

        public IEnumerable<double> GetPossibleReward(int directionIndex)
        {
            Direction direction = Enum.GetValues<Direction>()[directionIndex];
            if (((int)Direction % 2) == ((int)direction % 2))
            {
                yield return 0;
                yield break;
            }

            Tail tail = new()
            {
                X = Head.X + (((int)direction) % 2),
                Y = Head.Y + (((int)direction + 1) % 2)
            };

            if (Math.Sqrt(Math.Pow(_field.Apple.X - tail.X, 2) + Math.Pow(_field.Apple.Y - tail.Y, 2)) < Dictance(_field.Apple))
                yield return Rewards.GoToApple;
            else
                yield return Rewards.GoFromApple;

            if (Math.Sqrt(Math.Pow(_field.Apple.X - Head.X, 2) + Math.Pow(_field.Apple.Y - Head.Y, 2)) < 1d)
                yield return Rewards.EatApple;

            if (Tails.Any(_tail => tail.Intersect(_tail)))
                yield return Rewards.EatHimself;
            else if (tail.X < 0 || tail.X > Game.FieldWidth - 1 ||
                tail.Y < 0 || tail.Y > Game.FieldHeight - 1)
                yield return Rewards.DieByWall;
            else
                yield return Rewards.Live;
        }

        public bool Eat(Apple apple)
        {
            if (Head.Intersect(apple))
            {
                _shouldAdd = true;
                apple.GenerateApple();
                return true;
            }
            return false;
        }

        public double Dictance(Apple apple) =>
            Math.Sqrt(Math.Pow(apple.X - Head.X, 2) + Math.Pow(apple.Y - Head.Y, 2));

        public void Draw()
        {
            Console.BackgroundColor = ConsoleColor.Green;

            foreach (var tail in Tails)
                tail.Draw();

            Console.ResetColor();
        }

        public double IsDirectionEquals(Direction direction) => direction == Direction ? Game.HighValue : Game.LowValue;
        public double IsLeftSide(Apple apple) => Head.X >= apple.X ? Game.HighValue : Game.LowValue;
        public double IsRightSide(Apple apple) => Head.X <= apple.X ? Game.HighValue : Game.LowValue;
        public double IsUpSide(Apple apple) => Head.Y >= apple.Y ? Game.HighValue : Game.LowValue;
        public double IsDownSide(Apple apple) => Head.Y <= apple.Y ? Game.HighValue : Game.LowValue;
        public double IsWallUp() => Head.Y == 0 ? Game.HighValue : Game.LowValue;
        public double IsWallDown() => Head.Y >= Game.FieldHeight - 1 ? Game.HighValue : Game.LowValue;
        public double IsWallLeft() => Head.X == 0 ? Game.HighValue : Game.LowValue;
        public double IsWallRight() => Head.X >= Game.FieldWidth - 1 ? Game.HighValue : Game.LowValue;
    }

    public enum Direction
    {
        Left = -1,
        Up = -2,
        Right = 1,
        Down = 0
    }

    public class Tail
    {
        public int X { get; set; }
        public int Y { get; set; }

        public bool Intersect(Tail tail) => tail.X == X && tail.Y == Y;
        public bool Intersect(Apple apple) => apple.X == X && apple.Y == Y;

        public void Draw()
        {
            Console.SetCursorPosition(X, Y);
            Console.Write(" ");
        }
    }

    public class Apple
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Random Random { get; }

        public Apple()
        {
            Random = new();

            GenerateApple();
        }

        public void GenerateApple()
        {
            X = Random.Next(0, Game.FieldWidth);
            Y = Random.Next(0, Game.FieldHeight);
        }

        public void Draw()
        {
            Console.BackgroundColor = ConsoleColor.Red;

            Console.SetCursorPosition(X, Y);
            Console.Write(" ");

            Console.ResetColor();
        }
    }
}
