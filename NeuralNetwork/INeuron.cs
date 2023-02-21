namespace NeuralNetwork
{
    public interface INeuron : INeuronSignal, INeuronReceptor
    {
        void Pulse(INeuralLayer layer);
        void ApplyLearning(INeuralLayer layer);
        NeuralFactor Bias { get; set; }
        public double BiasWeight { get; set; }
        public double Error { get; set; }
    }

}
