using System;

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

            var random = new System.Random();
            for (var i = 0; i < weights.Length; i++) weights[i] = (float)(random.NextDouble() * 2.0 - 1.0);
            this.bias = bias;
            this.p = p;
        }

        public int WeightsCount => weights.Length;

        public float Synapsis(float[] input, int layer)
        {
            float a = 0;

            //if (input.Length > weights.Length)
            //{
            //    // TODO Borrar esto
            //    Debug.Log("Inputs " + input.Length + " Weights " + weights.Length + ". problem in layer " + layer);
            //    return 0;
            //}

            for (int i = 0; i < input.Length; i++)
            {
                a += weights[i] * input[i];
            }

            a += bias;

            return Sigmoid(a);
        }

        public int SetWeights(float[] newWeights, int fromId)
        {
            for (var i = 0; i < weights.Length; i++) weights[i] = newWeights[i + fromId];

            return fromId + weights.Length;
        }
        
        private float Tanh(float a)
        {
            return (float)Math.Tanh(a);
        }


        public float[] GetWeights()
        {
            return weights;
        }

        private float Sigmoid(float a)
        {
            return 1.0f / (1.0f + (float)Math.Exp(-a / p));
        }
    }
}