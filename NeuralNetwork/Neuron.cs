using System;
using System.Collections.Generic;
using System.Linq;

namespace NeuralNetwork
{
    public class Neuron : INeuron
    {
        public double Output { get; set; }
        public Dictionary<INeuronSignal, NeuralFactor> Input { get; }
        public void Pulse(INeuralLayer layer)
        {
            lock (this)
            {
                Output = Input.Sum(item => item.Key.Output * item.Value.Weight);
                Output += Bias.Weight * BiasWeight;
                Output = Sigmoid(Output);
            }
        }

        public void ApplyLearning(INeuralLayer layer)
        {
            
        }

        public NeuralFactor Bias { get; set; }
        public double BiasWeight { get; set; }
        public double Error { get; set; }

        private static double Sigmoid(double value)
        {
            return 1 / (1 + Math.Exp(-value));
        }
    }
}
