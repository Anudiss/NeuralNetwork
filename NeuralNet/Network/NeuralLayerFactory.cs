namespace NeuralNet.Network
{
    public class NeuralLayerFactory
    {
        public NeuralLayer CreateNeuralLayer(int numberOfInputNeurons, IActivationFunction activationFunction, IInputFunction inputFunction)
        {
            var layer = new NeuralLayer();

            for (int i = 0; i < numberOfInputNeurons; i++)
                layer.Neurons.Add(new Neuron(activationFunction, inputFunction));

            return layer;
        }
    }
}
