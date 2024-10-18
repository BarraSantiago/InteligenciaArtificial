using ECS.Patron;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeuralNetworkDirectory.ECS
{
    public sealed class NeuralNetSystem : ECSSystem
    {
        private ParallelOptions parallelOptions;
        private IDictionary<uint, NeuralNetComponent> neuralNetworkComponents;
        private IDictionary<uint, OutputComponent> outputComponents;
        private IDictionary<uint, InputComponent> inputComponents;
        private IEnumerable<uint> queriedEntities;

        public override void Initialize()
        {
            parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
        }

        protected override void PreExecute(float deltaTime)
        {
            neuralNetworkComponents ??= ECSManager.GetComponents<NeuralNetComponent>();
            outputComponents ??= ECSManager.GetComponents<OutputComponent>();
            inputComponents ??= ECSManager.GetComponents<InputComponent>();
            queriedEntities ??= ECSManager.GetEntitiesWithComponentTypes(typeof(NeuralNetComponent),
                typeof(NeuronComponent), typeof(OutputComponent), typeof(InputComponent));
        }

        protected override void Execute(float deltaTime)
        {
            Parallel.ForEach(queriedEntities, parallelOptions, entityId =>
            {
                var neuralNetwork = neuralNetworkComponents[entityId];
                var inputs = inputComponents[entityId].inputs;
                float[] outputs = new float[outputComponents[entityId].outputs.Length];

                for (int i = 0; i < outputs.Length; i++)
                {
                    outputs = neuralNetwork.Layers[i].Synapsis(inputs);
                    inputs = outputs;
                }

                outputComponents[entityId].outputs = outputs;
            });
        }

        protected override void PostExecute(float deltaTime)
        {
        }
    }
}