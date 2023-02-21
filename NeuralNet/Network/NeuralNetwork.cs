using NeuralNet.GameLogic;
using System.Threading.Tasks.Sources;

namespace NeuralNet.Network
{
    public class NeuralNetwork
    {
        private NeuralLayerFactory _layerFactory;
        public const double LearningRate = 2.95;

        public List<NeuralLayer> layers;

        public NeuralLayer InputLayer => layers[0];
        public NeuralLayer OutputLayer => layers.Last();

        public IEnumerable<NeuralLayer> HiddenLayers => layers.Except(new[] { InputLayer, OutputLayer });

        public double[] Expectation;

        public NeuralNetwork(int inputNeuronNumbers)
        {
            layers = new();
            _layerFactory = new();

            CreateInputLayer(inputNeuronNumbers);
        }

        public void Train(Game game)
        {
            var learningRate = .9;
            var discount = .5;

            for (int i = 0; i < Expectation.Length; i++)
                Expectation[i] = (1 - learningRate) * Expectation[i] + learningRate * (game.Score + discount * game.Field.Snake.GetPossibleReward(i).Max());
            //Expectation[i] += 1d / (game.ActionsCount[game.Field.Snake.Direction] + 1d) * (game.Score - Expectation[i]);

            double[] expected = new double[Expectation.Length];

            int maxExcectationIndex = Expectation.ToList().IndexOf(Expectation.Max());
            for (int i = 0; i < expected.Length; i++)
            {
                if (i == maxExcectationIndex)
                    expected[i] = 0;
                else
                    expected[i] = 1;
            }

            for (int i = 0; i < 10000; i++)
            {
                CalculateErrors(expected);
            }

            //input.Weight = (1 - learningRate) * input.Weight + learningRate * (game.Score + discount * Enumerable.Range(0, 4).Select(i => Enum.GetValues<Direction>()[i]).Select(direction => game.Field.Snake.GetReward(direction)).Max());
            //output.Weight += 1d / (game.ActionsCount[game.Field.Snake.Direction] + 1d) * (game.Score - game.Field.Snake.GetReward(game.Field.Snake.LastDirection));
        }

        public void Train(double[][] inputs, double[][] output, int numberOfEpochs)
        {
            double totalError = 0;

            for (int i = 0; i < numberOfEpochs; i++)
            {
                for (int j = 0; j < inputs.Length; j++)
                {
                    PushInputValues(inputs[j]);

                    var outputs = new List<double>();

                    // Get outputs.
                    layers.Last().Neurons.ForEach(x =>
                    {
                        outputs.Add(x.CalculateOutput());
                    });

                    // Calculate error by summing errors on all output neurons.
                    CalculateErrors(output[j]);
                }
            }
        }

        private void CalculateErrors(double[] desiredResults)
        {
            #region Declarations

            double temp, error;
            INeuron outputNode, hiddenNode;

            #endregion

            #region Execution

            double[][] errors = new double[layers.Count - 1][];
            errors[0] = new double[OutputLayer.Neurons.Count];

            // Calcualte output error values
            for (int i = 0; i < OutputLayer.Neurons.Count; i++)
            {
                outputNode = OutputLayer.Neurons[i];
                temp = outputNode.CalculateOutput();

                errors[0][i] = (desiredResults[i] - temp) * SigmoidDerivative(temp); //* temp * (1.0F - temp);
            }


            // calculate hidden layer error values
            for (int k = HiddenLayers.Count() - 1; k >= 0; k--)
            {
                var layer = HiddenLayers.ElementAt(k);
                var previousLayer = layers[k + 1];

                errors[layers.Count - k - 2] = new double[layer.Neurons.Count];

                for (int i = 0; i < layer.Neurons.Count; i++)
                {
                    hiddenNode = layer.Neurons[i];
                    temp = hiddenNode.CalculateOutput();

                    error = 0;
                    for (int j = 0; j < previousLayer.Neurons.Count; j++)
                    {
                        outputNode = previousLayer.Neurons[j];
                        error += (errors[layers.Count - k - 2][j] * outputNode.Inputs[i].Weight) * SigmoidDerivative(temp);// *(1.0F - temp);                   
                    }

                    errors[layers.Count - k - 2][i] = error;

                }
            }


            #endregion

            #region Execution

            for (int i = layers.Count - 1; i > 0; i--)
            {
                var layer = layers[i];
                var nextLayer = layers[i - 1];

                for (int j = 0; j < layer.Neurons.Count; j++)
                {
                    outputNode = layer.Neurons[j];

                    for (int k = 0; k < nextLayer.Neurons.Count; k++)
                    {
                        var node = nextLayer.Neurons[k];
                        outputNode.Inputs[k].Weight += errors[layers.Count - i - 1][j] * node.CalculateOutput();
                    }
                }
            }

            #endregion
        }

        public void HandleOutputLayer(int row, double[][] excepted)
        {
            layers.Last().Neurons.ForEach(neuron =>
            {
                neuron.Inputs.ForEach(connection =>
                {
                    var netInput = connection.GetOutput();
                    var output = neuron.CalculateOutput();
                    var expectedOutput = excepted[row][layers.Last().Neurons.IndexOf(neuron)];

                    var nodeDelta = (expectedOutput - output) * output * (1 - output);
                    var delta = -1 * netInput * nodeDelta;

                    connection.UpdateWeight(2.95, delta);

                    neuron.PreviousPartialDerivate = nodeDelta;
                });
            });
        }

        public void HandleHiddenLayers()
        {
            for (int k = layers.Count - 2; k > 0; k--)
            {
                layers[k].Neurons.ForEach(neuron =>
                {
                    neuron.Inputs.ForEach(connection =>
                    {
                        var output = neuron.CalculateOutput();
                        var netInput = connection.GetOutput();
                        double sumPartial = 0;

                        layers[k + 1].Neurons
                        .ForEach(outputNeuron =>
                        {
                            outputNeuron.Inputs.Where(i => i.IsFromNeuron(neuron.Id))
                            .ToList()
                            .ForEach(outConnection =>
                            {
                                sumPartial += outConnection.PreviousWeight * outputNeuron.PreviousPartialDerivate;
                            });
                        });

                        var delta = -1 * netInput * sumPartial * output * (1 - output);
                        connection.UpdateWeight(2.95, delta);
                    });
                });
            }
        }

        public void AddLayer(NeuralLayer layer)
        {
            if (layers.Any())
                layer.ConnectLayers(layers.Last());

            layers.Add(layer);

            Expectation = Enumerable.Range(0, layer.Neurons.Count).Select(e => new Random().NextDouble()).ToArray();
        }

        public void PushInputValues(double[] inputs) =>
            layers.First().Neurons.ForEach(x => x.PushValueOnInput(inputs[layers.First().Neurons.IndexOf(x)]));

        public List<double> GetOutput()
        {
            var returnValue = new List<double>();

            layers.Last().Neurons.ForEach(neuron =>
            {
                returnValue.Add(neuron.CalculateOutput());
            });

            return returnValue;
        }

        private static double SigmoidDerivative(double value) => value * (1.0F - value);
        private static double Sigmoid(double value) => 1 / (1 + Math.Exp(-value));

        private void CreateInputLayer(int numberOfInputNeurons)
        {
            var inputLayer = _layerFactory.CreateNeuralLayer(numberOfInputNeurons, new RectifiedActivationFuncion(), new WeightedSumFunction());
            inputLayer.Neurons.ForEach(x => x.AddInputSynapse(0));
            this.AddLayer(inputLayer);
        }
    }
}