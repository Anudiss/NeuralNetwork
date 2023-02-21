using NeuralNet.Network;

namespace NeuralNet.GameLogic
{
    public class Game
    {
        public const int FieldWidth = 40;
        public const int FieldHeight = 20;

        public const double MinScore = -500;
        public const double MaxScore = 2000;

        public const int InitialSnakeLength = 4;
        public static bool ShouldDraw = false;

        public const double HighValue = .9;
        public const double LowValue = .1;

        public static int UpdateTime = 50;

        public Dictionary<Direction, int> ActionsCount = new()
        {
            { Direction.Up, 0 },
            { Direction.Left, 0 },
            { Direction.Right, 0 },
            { Direction.Down, 0 }
        };

        public bool IsRunning { get; set; } = false;
        public DateTime LastTime = DateTime.MinValue;

        public Field Field { get; }

        public double LastScore = 0;
        public double Score
        {
            get => Field.Snake.Score;
            set => Field.Snake.Score = value;
        }
        public double LastAppleDictance { get; set; }

        public NeuralNetwork Network { get; set; }

        public Game()
        {
            Console.CursorVisible = false;

            Field = new(FieldWidth, FieldHeight);

            Network = new(12);
            var layerFactory = new NeuralLayerFactory();

            Network.AddLayer(layerFactory.CreateNeuralLayer(5, new RectifiedActivationFuncion(), new WeightedSumFunction()));
            Network.AddLayer(layerFactory.CreateNeuralLayer(5, new RectifiedActivationFuncion(), new WeightedSumFunction()));
            Network.AddLayer(layerFactory.CreateNeuralLayer(5, new RectifiedActivationFuncion(), new WeightedSumFunction()));
            Network.AddLayer(layerFactory.CreateNeuralLayer(5, new RectifiedActivationFuncion(), new WeightedSumFunction()));
            Network.AddLayer(layerFactory.CreateNeuralLayer(4, new SigmoidActivationFunction(), new WeightedSumFunction()));

            LastAppleDictance = Field.Snake.Dictance(Field.Apple);
        }

        public void NetworkMove()
        {
            Field.Snake.Direction = GetNetworkOutput();
        }

        public Direction GetNetworkOutput()
        {
            Network.PushInputValues(new double[] 
            { 
                Field.Snake.IsLeftSide(Field.Apple),
                Field.Snake.IsUpSide(Field.Apple),
                Field.Snake.IsRightSide(Field.Apple),
                Field.Snake.IsDownSide(Field.Apple),
                Field.Snake.IsWallUp(),
                Field.Snake.IsWallLeft(),
                Field.Snake.IsWallDown(),
                Field.Snake.IsWallRight(),
                Field.Snake.IsDirectionEquals(Direction.Left),
                Field.Snake.IsDirectionEquals(Direction.Up),
                Field.Snake.IsDirectionEquals(Direction.Right),
                Field.Snake.IsDirectionEquals(Direction.Down)
            });

            List<double> output = Network.GetOutput();
            double max = output.Max();

            Console.SetCursorPosition(FieldWidth + 2, 2);
            Console.Write($"Total:   {Score}");

            Console.SetCursorPosition(FieldWidth + 2, 3);
            Console.Write($"Highest: {Program.Highest}");

            Console.SetCursorPosition(FieldWidth + 2, 4);
            Console.Write($"Attempt: {Program.Attempt}");

            Console.SetCursorPosition(FieldWidth + 2, 5);
            Console.Write($"Ожидание: [ {string.Join(", ", Network.Expectation.Select(e => $"{e:F5}"))} ]");

            Direction newDirection = Enum.GetValues<Direction>()[output.IndexOf(max)];
            ActionsCount[newDirection]++;
            return newDirection;
        }

        public void Start()
        {
            if (IsRunning)
                return;

            IsRunning = true;
        }

        public void Tick()
        {
            if ((DateTime.Now - LastTime).TotalMilliseconds >= UpdateTime)
            {
                Draw();
                Update();
                LastTime = DateTime.Now;
            }
        }

        private void Update()
        {
            if (!Field.Snake.IsAlive)
                return;

            NetworkMove();

            Field.Snake.Move();

            var appleDictance = Field.Snake.Dictance(Field.Apple);
            if (appleDictance < LastAppleDictance)
                Score += Rewards.GoToApple;
            else
                Score += Rewards.GoFromApple;

            LastAppleDictance = appleDictance;

            if (Field.Snake.Eat(Field.Apple))
                Score += Rewards.EatApple;
/*
            if (Score <= MinScore || Score >= MaxScore)
                Field.Snake.IsAlive = false;*/

            if (!Field.Snake.IsAlive)
            {
                Program.Highest = Math.Max(Program.Highest, Score);
                Console.Title = $"Dead | Score: {Score}";
                IsRunning = false;
            }
        }

        public void Draw()
        {
            if (!ShouldDraw)
                return;

            Console.Clear();

            Field.Draw();
        }
    }
}
