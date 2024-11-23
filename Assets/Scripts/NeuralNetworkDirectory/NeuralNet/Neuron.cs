using System;

namespace NeuralNetworkDirectory.NeuralNet
{
    public class Neuron
    {
        private readonly float bias;
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

        public float Synapsis(float[] input)
        {
            float a = 0;

            for (int i = 0; i < input.Length; i++)
            {
                a += weights[i] * input[i];
            }

            a += bias;

            return Tanh(a);
        }
        
        private float Tanh(float a)
        {
            return (float)Math.Tanh(a);
        }

        private float Sigmoid(float a)
        {
            return 1.0f / (1.0f + (float)Math.Exp(-a / p));
        }
    }
}