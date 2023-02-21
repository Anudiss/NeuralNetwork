namespace NeuralNet.Network
{
    public interface IInputFunction
    {
        double CalculateInput(List<ISynapse> inputs);
    }
}
