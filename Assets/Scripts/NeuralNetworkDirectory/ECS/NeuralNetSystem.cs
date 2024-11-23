using System;
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

        public override void Deinitialize()
        {
            neuralNetworkComponents = null;
            outputComponents = null;
            inputComponents = null;
            queriedEntities = null;
        }

        protected override void PreExecute(float deltaTime)
        {
            neuralNetworkComponents ??= ECSManager.GetComponents<NeuralNetComponent>();
            outputComponents ??= ECSManager.GetComponents<OutputComponent>();
            inputComponents ??= ECSManager.GetComponents<InputComponent>();
            queriedEntities ??= ECSManager.GetEntitiesWithComponentTypes(
                typeof(NeuralNetComponent), typeof(OutputComponent), typeof(InputComponent));
        }

        protected override void Execute(float deltaTime)
        {
            const int MaxBrains = 3;
            Parallel.ForEach(queriedEntities, parallelOptions, entityId =>
            {
                Parallel.For(0, MaxBrains, i =>
                {
                    NeuralNetComponent neuralNetwork = neuralNetworkComponents[entityId];
                    float[][] inputs = inputComponents[entityId].inputs;
                    float[] outputs = new float[3];
                    for (int j = 0; j < neuralNetwork.Layers[i].Count; j++)
                    {
                        outputs = neuralNetwork.Layers[i][j].Synapsis(inputs[i]);
                        inputs[i] = outputs;
                    }

                    if (neuralNetwork.Layers[i][^1].OutputsCount != outputs.Length) return;

                    outputComponents[entityId].Outputs[i] = outputs;
                });
            });
        }

        protected override void PostExecute(float deltaTime)
        {
        }
    }
}