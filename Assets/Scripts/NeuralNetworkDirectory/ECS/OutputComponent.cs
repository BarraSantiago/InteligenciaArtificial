using ECS.Patron;

namespace NeuralNetworkDirectory.ECS
{
    public class OutputComponent : ECSComponent
    {
        public int outputsQty;
        public float[][] outputs;
    }
}