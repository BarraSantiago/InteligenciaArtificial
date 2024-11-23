using System;

namespace NeuralNetworkDirectory.NeuralNet
{
    public class Neuron
    {
        public readonly float bias;
        private readonly float p;
        public readonly float[] weights;

        public Neuron(int weightsCount, float bias, float p)
        {
            weights = new float[weightsCount];

            Random random = new System.Random();
            for (int i = 0; i < weights.Length; i++) weights[i] = (float)(random.NextDouble() * 2.0 - 1.0);
            this.bias = bias;
            this.p = p;
        }
    }
}