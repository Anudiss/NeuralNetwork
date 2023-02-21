using System.Collections.Generic;

namespace NeuralNetwork
{
    public interface INeuronReceptor
    {
        Dictionary<INeuronSignal, NeuralFactor> Input { get; }
    }

}
