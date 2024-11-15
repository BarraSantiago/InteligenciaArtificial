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
        private Neuron[] neurons;
        private float[] outputs;
        private int totalWeights;

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

            for (var i = 0; i < neurons.Length; i++)
            {
                neurons[i] = new Neuron(InputsCount, Bias, p);
                totalWeights += InputsCount;
            }

            outputs = new float[neurons.Length];
        }

        public int SetWeights(float[] weights, int fromId)
        {
            for (var i = 0; i < neurons.Length; i++) fromId = neurons[i].SetWeights(weights, fromId);

            return fromId;
        }

        public float[] GetWeights()
        {
            var weights = new float[totalWeights];
            var id = 0;

            for (var i = 0; i < neurons.Length; i++)
            {
                var ws = neurons[i].GetWeights();

                for (var j = 0; j < ws.Length; j++)
                {
                    weights[id] = ws[j];
                    id++;
                }
            }

            return weights;
        }

        public float[] Synapsis(float[] inputs, int i)
        {
            Parallel.For(0, neurons.Length, j =>
            {
                outputs[j] = neurons[j].Synapsis(inputs, i);
            });
            return outputs;
        }
    }
}