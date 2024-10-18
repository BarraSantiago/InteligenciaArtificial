using UnityEngine;

namespace NeuralNetworkDirectory.NeuralNet
{
    public class Neuron
    {
        private readonly float bias;
        private readonly float p;
        private readonly float[] weights;

        public Neuron(int weightsCount, float bias, float p)
        {
            weights = new float[weightsCount];

            for (var i = 0; i < weights.Length; i++) weights[i] = Random.Range(-1.0f, 1.0f);

            this.bias = bias;
            this.p = p;
        }

        public int WeightsCount => weights.Length;

        public float Synapsis(float[] input)
        {
            float a = 0;

            for (var i = 0; i < input.Length; i++) a += weights[i] * input[i];

            a += bias * weights[weights.Length - 1];

            return Sigmoid(a);
        }

        public int SetWeights(float[] newWeights, int fromId)
        {
            for (var i = 0; i < weights.Length; i++) weights[i] = newWeights[i + fromId];

            return fromId + weights.Length;
        }

        public float[] GetWeights()
        {
            return weights;
        }

        private float Sigmoid(float a)
        {
            return 1.0f / (1.0f + Mathf.Exp(-a / p));
        }
    }
}