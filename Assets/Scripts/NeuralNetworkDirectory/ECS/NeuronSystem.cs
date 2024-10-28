using ECS.Patron;

namespace NeuralNetworkDirectory.ECS
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    namespace NeuralNetworkDirectory.ECS
    {
        public sealed class NeuronSystem : ECSSystem
        {
            private ParallelOptions parallelOptions;
            private IDictionary<uint, NeuronComponent> neuronComponents;
            private IEnumerable<uint> queriedEntities;

            public override void Initialize()
            {
                parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
            }

            protected override void PreExecute(float deltaTime)
            {
                neuronComponents ??= ECSManager.GetComponents<NeuronComponent>();
                queriedEntities ??= ECSManager.GetEntitiesWithComponentTypes(typeof(NeuronComponent));
            }

            protected override void Execute(float deltaTime)
            {
                // TODO implement input and output
                float[] inputs = new float[] { };

                Parallel.ForEach(queriedEntities, parallelOptions, entityId =>
                {
                    var neuron = neuronComponents[entityId];
                    float output = Synapsis(neuron, inputs);
                });
            }

            private float Synapsis(NeuronComponent neuron, float[] inputs)
            {
                float a = 0;
                for (int i = 0; i < inputs.Length; i++)
                {
                    a += neuron.Weights[i] * inputs[i];
                }
                a += neuron.Bias * neuron.Weights[^1];
                return Sigmoid(a, neuron.P);
                
            }

            private static float Sigmoid(float a, float p)
            {
                return 1.0f / (1.0f + (float)Math.Exp(-a / p));
            }

            protected override void PostExecute(float deltaTime)
            {
            }
        }
    }
}