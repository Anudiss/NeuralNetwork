using NeuralNet.GameLogic;
using NeuralNet.Network;
using System.Reflection.Metadata.Ecma335;

namespace NeuralNet;

public class Program
{
    public static double Highest { get; set; } = double.MinValue;
    public static int Attempt { get; set; } = 0;

    public static void Main(string[] args)
    {
        RunSingleNetwork();
    }

    private static void BinaryNetworkTrain()
    {
        var network = new NeuralNetwork(3);
        var layerFactory = new NeuralLayerFactory();

        network.AddLayer(layerFactory.CreateNeuralLayer(2, new RectifiedActivationFuncion(), new WeightedSumFunction()));
        network.AddLayer(layerFactory.CreateNeuralLayer(1, new SigmoidActivationFunction(), new WeightedSumFunction()));

        // a | b & c
        double[][] training_input = new double[][]
        {
            new double[] { 0, 0, 0 },
            new double[] { 0, 1, 0 },
            new double[] { 1, 1, 1 },
            new double[] { 0, 1, 1 }
        };

        double[][] training_output = new double[][]
        {
            new double[] { 0 },
            new double[] { 0 },
            new double[] { 1 },
            new double[] { 1 }
        };


        double[][] final_tests = new double[][]
        {
            new double[] { 1, 1, 1 },
            new double[] { 0, 1, 1 },
            new double[] { 0, 1, 0 }
        };

        for (int i = 0; i < 10; i++)
        {
            network.Train(training_input, training_output, 100000);


            Console.WriteLine($"=========================Test #${i}=========================");
            foreach (var final_test in final_tests)
            {
                network.PushInputValues(final_test);

                Console.WriteLine($"{final_test[0]} | {final_test[1]} & {final_test[2]} = {network.GetOutput()[0]}");
            }
        }

        Console.ReadKey(true);
    }

    private static void RunNetworkTrainingWithTeacher()
    {
        ConsoleKey[] possibleKeys = new[] { ConsoleKey.UpArrow, ConsoleKey.LeftArrow, ConsoleKey.DownArrow, ConsoleKey.RightArrow };

        Game game = new();

        Game.ShouldDraw = true;
        Game.UpdateTime = 0;

        game.Start();

        double[] input = new double[]
        {
            1, 0, 0, 1,
            0, 0, 0, 0,
            0, 1, 0, 0
        };

        double[] excepted1 = new double[]
        {
            0, 0, 1, 0
        };

        game.Network.PushInputValues(input);

        var a = game.Network.GetOutput();

        for (int i = 0; i < 1000; i++)
        {
            /*game.Network.HandleOutputLayer(excepted1);*/
            game.Network.HandleHiddenLayers();
        }

        var b = game.Network.GetOutput();

        ConsoleKeyInfo keyInfo;

        do
        {
            keyInfo = Console.ReadKey(true);

            if (possibleKeys.Contains(keyInfo.Key) == false)
                continue;

            double[] excepted = new double[]
            {
                keyInfo.Key == ConsoleKey.LeftArrow ? 1 : 0,
                keyInfo.Key == ConsoleKey.UpArrow ? 1 : 0,
                keyInfo.Key == ConsoleKey.RightArrow ? 1 : 0,
                keyInfo.Key == ConsoleKey.DownArrow ? 1 : 0
            };

            int iteration = 0;
            while(!IsSimillar(excepted, game.Network.GetOutput().ToArray()))
            {
                /*game.Network.HandleOutputLayer(excepted);*/
                game.Network.HandleHiddenLayers();

                Console.Title = $"Learning: {++iteration} time";
            }

            game.Tick();

            if (game.Field.Snake.IsAlive == false)
            {
                game.Field.Restart();
                game.Score = 0;
                game.Start();
            }

        }
        while (keyInfo.Key != ConsoleKey.Escape);
    }

    private static bool IsSimillar(double[] excepted, double[] output)
    {
        return output[excepted.ToList().IndexOf(1)] == output.Max();
    }

    private static void TrainThenRunBest()
    {
        Game[] games = new Game[1];

        for (int j = 0; j < games.Length; j++)
        {
            games[j] = new();
            games[j].Start();
        }

        Game.ShouldDraw = false;
        Game.UpdateTime = 0;
        for (int i = 0; i < 10000; i++)
        {
            while (games.Any(game => game.IsRunning))
            {
                foreach (Game game in games)
                {
                    while (game.IsRunning)
                    {
                        game.Tick();
                        game.Network.Train(game);
                        game.Score = 0;
                    }

                    game.Field.Restart();
                    game.Score = 0;
                }
            }

            Attempt = i;

            foreach (var game in games)
                game.Start();
        }

        double maxScore = games.Max(game => game.Score);
        Game best = games.First(game => game.Score == maxScore);

        Game.UpdateTime = 150;
        Game.ShouldDraw = true;

        while (true)
        {
            best.Field.Restart();
            best.Score = 0;
            best.Start();

            while (best.IsRunning)
                best.Tick();

            Console.ReadKey();
        }
    }

    private static void RunSingleNetwork()
    {
        Game.ShouldDraw = true;
        Game.UpdateTime = 10;

        Game game = new();

        game.Start();

        while (true)
        {
            while (game.IsRunning)
            {
                game.Tick();
                game.Network.Train(game);
                game.Score = 0;
            }


            game.Field.Restart();
            game.Score = 0;

            game.Start();

            Attempt++;
        }

        Console.ReadKey();
    }

    private static void RunNetworkTraining()
    {
        Game[] games = new Game[200];

        for (int i = 0; i < games.Length; i++)
        {
            games[i] = new();
            games[i].Start();
        }

        while (games.Any(game => game.IsRunning))
        {
            foreach (Game game in games)
            {
                game.Tick();
            }
        }

        Console.WriteLine($"Best: {games.Max(game => game.Score)} | Worst: {games.Min(game => game.Score)}");

        Console.ReadKey(true);

        while (games.Any(game => game.IsRunning))
        {
            foreach (Game game in games)
            {
                game.Tick();
            }
        }

        Console.WriteLine($"Best: {games.Max(game => game.Score)} | Worst: {games.Min(game => game.Score)}");

        Console.ReadKey(true);
    }

    public static NeuralNetwork InitializeNetwork()
    {
        var network = new NeuralNetwork(2);
        var layerFactory = new NeuralLayerFactory();

        network.AddLayer(layerFactory.CreateNeuralLayer(3, new RectifiedActivationFuncion(), new WeightedSumFunction()));
        network.AddLayer(layerFactory.CreateNeuralLayer(3, new SigmoidActivationFunction(), new WeightedSumFunction()));

        return network;
    }
    
}