using System;
using System.Collections;
using System.Collections.Generic;

namespace NeuralNetwork
{
    public interface INeuralLayer : IList<INeuron>
    {
        void Pulse(INeuralNet net);
        void ApplyLearning(INeuralNet net);
    }

    public interface INeuralNet
    {
        public INeuralLayer HiddenLayer { get; set; }
        public INeuralLayer PerceptionLayer { get; set; }
        public INeuralLayer OutputLayer { get; set; }

        public void ApplyLearning();
        public void Pulse();
    }

    public class NeuralNet : INeuralNet
    {
        public INeuralLayer HiddenLayer { get; set; }
        public INeuralLayer InputLayer { get; set; }
        public INeuralLayer PerceptionLayer { get; set; }
        public INeuralLayer OutputLayer { get; set; }
        public double LearningRate { get; set; } = 1;

        public NeuralNet()
        {
            
        }

        public void Initialize(int randomSeed, int inputNeuronCount, int hiddenNeuronCount, int outputNeuronCount)
        {
            int i, j, k, layerCount;
            Random rand;
            INeuralLayer layer;
            // initializations  
            rand = new Random(randomSeed);
            InputLayer = new NeuralLayer();
            OutputLayer = new NeuralLayer();
            HiddenLayer = new NeuralLayer();
            for (i = 0; i < inputNeuronCount; i++)
                InputLayer.Add(new Neuron());
            for (i = 0; i < outputNeuronCount; i++)
                OutputLayer.Add(new Neuron());
            for (i = 0; i < hiddenNeuronCount; i++)
                HiddenLayer.Add(new Neuron());
            // wire-up input layer to hidden layer  
            for (i = 0; i < HiddenLayer.Count; i++)
            for (j = 0; j < InputLayer.Count; j++)
                HiddenLayer[i].Input.Add(InputLayer[j],
                    new NeuralFactor(rand.NextDouble()));
            // wire-up output layer to hidden layer  
            for (i = 0; i < OutputLayer.Count; i++)
            for (j = 0; j < HiddenLayer.Count; j++)
                OutputLayer[i].Input.Add(HiddenLayer[j],
                    new NeuralFactor(rand.NextDouble()));
        }

        private void BackPropogation(double[] desiredResults)
        {
            int i, j;
            double temp, error;
            INeuron outputNode, inputNode, hiddenNode, node, node2;
            // Calcualte output error values  
            for (i = 0; i < OutputLayer.Count; i++)
            {
                temp = OutputLayer[i].Output;
                OutputLayer[i].Error = (desiredResults[i] - temp) * temp * (1.0F - temp);
            }
            // calculate hidden layer error values  
            for (i = 0; i < HiddenLayer.Count; i++)
            {
                node = HiddenLayer[i];
                error = 0;
                for (j = 0; j < OutputLayer.Count; j++)
                {
                    outputNode = OutputLayer[j];
                    error += outputNode.Error * outputNode.Input[node].Weight * node.Output * (1.0 - node.Output);
                }
                node.Error = error;
            }
            // adjust output layer weight change  
            for (i = 0; i < HiddenLayer.Count; i++)
            {
                node = HiddenLayer[i];
                for (j = 0; j < OutputLayer.Count; j++)
                {
                    outputNode = OutputLayer[j];
                    outputNode.Input[node].Weight += LearningRate * OutputLayer[j].Error * node.Output;
                    outputNode.Bias.Delta += LearningRate * OutputLayer[j].Error * outputNode.Bias.Weight;
                }
            }
            // adjust hidden layer weight change  
            for (i = 0; i < InputLayer.Count; i++)
            {
                inputNode = InputLayer[i];
                for (j = 0; j < HiddenLayer.Count; j++)
                {
                    hiddenNode = HiddenLayer[j];
                    hiddenNode.Input[inputNode].Weight += LearningRate * hiddenNode.Error * inputNode.Output;
                    hiddenNode.Bias.Delta += LearningRate * hiddenNode.Error * inputNode.Bias.Weight;
                }
            }
        }


        public void ApplyLearning()
        {
            lock (this)
            {
                HiddenLayer.ApplyLearning(this);
                OutputLayer.ApplyLearning(this);
            }
        }

        public void Pulse()
        {
            lock (this)
            {
                HiddenLayer.Pulse(this);
                OutputLayer.Pulse(this);
            }
        }

        public void Train(double[] input, double[] desiredResult)
        {
            int i;
            // initialize data  
            for (i = 0; i < InputLayer.Count; i++)
            {
                Neuron n = InputLayer[i] as Neuron;
                if (null != n) // maybe make interface get;set;  
                    n.Output = input[i];
            }
            Pulse();
            BackPropogation(desiredResult);
        }
        public void Train(double[][] inputs, double[][] expected)
        {
            for (int i = 0; i < inputs.Length; i++)
                Train(inputs[i], expected[i]);
        }
    }

    public class NeuralLayer : INeuralLayer
    {
        private List<INeuron> m_neurons = new();

        public IEnumerator<INeuron> GetEnumerator() => m_neurons.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(INeuron item) => m_neurons.Add(item);

        public void Clear() => m_neurons.Clear();

        public bool Contains(INeuron item) => m_neurons.Contains(item);

        public void CopyTo(INeuron[] array, int arrayIndex) => m_neurons.CopyTo(array, arrayIndex);

        public bool Remove(INeuron item) => m_neurons.Remove(item);

        public int Count { get; }
        public bool IsReadOnly { get; }
        public int IndexOf(INeuron item) => m_neurons.IndexOf(item);

        public void Insert(int index, INeuron item) => m_neurons.Insert(index, item);

        public void RemoveAt(int index) => m_neurons.RemoveAt(index);

        public INeuron this[int index]
        {
            get => m_neurons[index];
            set => m_neurons[index] = value;
        }

        public void Pulse(INeuralNet net)
        {
            foreach (var n in m_neurons)
                n.Pulse(this);
        }

        public void ApplyLearning(INeuralNet net)
        {
            foreach (var n in m_neurons)
                n.ApplyLearning(this);
        }
    }

}
