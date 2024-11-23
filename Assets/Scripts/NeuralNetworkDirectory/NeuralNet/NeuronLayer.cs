using System.Threading.Tasks;
using StateMachine.Agents.Simulation;

namespace NeuralNetworkDirectory.NeuralNet
{
    public enum BrainType
    {
        Movement,
        ScavengerMovement,
        Eat,
        Attack,
        Escape,
        Flocking
    }
    
    public class NeuronLayer
    {
        public BrainType BrainType;
        public SimAgentTypes AgentType;
        public float Bias {  get; set; }= 1;
        private readonly float p = 0.5f;
        public Neuron[] neurons;
        private float[] outputs;
        private int totalWeights;
        private ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = 32
        };

        public NeuronLayer(int inputsCount, int neuronsCount, float bias, float p)
        {
            InputsCount = inputsCount;
            this.Bias = bias;
            this.p = p;

            SetNeuronsCount(neuronsCount);
        }

        public int NeuronsCount => neurons.Length;

        public int InputsCount { get; }

        public int OutputsCount => outputs.Length;

        private void SetNeuronsCount(int neuronsCount)
        {
            neurons = new Neuron[neuronsCount];

            for (int i = 0; i < neurons.Length; i++)
            {
                neurons[i] = new Neuron(InputsCount, Bias, p);
                totalWeights += InputsCount;
            }

            outputs = new float[neurons.Length];
        }
    }
}