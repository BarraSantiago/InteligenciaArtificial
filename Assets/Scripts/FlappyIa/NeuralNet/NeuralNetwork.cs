using System.Collections.Generic;

namespace FlappyIa.NeuralNet
{
    public class NeuralNetwork
    {
        List<NeuronLayer> layers = new List<NeuronLayer>();
        int totalWeightsCount = 0;
        int inputsCount = 0;

        public int InputsCount
        {
            get { return inputsCount; }
        }

        public bool AddNeuronLayer(int neuronsCount, float bias, float p)
        {
            if (layers.Count == 0)
                return false;

            return AddNeuronLayer(layers[^1].OutputsCount, neuronsCount, bias, p);
        }

        public bool AddFirstNeuronLayer(int inputsCount, float bias, float p)
        {
            if (layers.Count != 0)
                return false;

            this.inputsCount = inputsCount;

            return AddNeuronLayer(inputsCount, inputsCount, bias, p);
        }

        private bool AddNeuronLayer(int inputsCount, int neuronsCount, float bias, float p)
        {
            if (layers.Count > 0 && layers[^1].OutputsCount != inputsCount)
                return false;

            NeuronLayer layer = new NeuronLayer(inputsCount, neuronsCount, bias, p);

            totalWeightsCount += (inputsCount + 1) * neuronsCount;

            layers.Add(layer);

            return true;
        }

        public int GetTotalWeightsCount()
        {
            return totalWeightsCount;
        }

        public void SetWeights(float[] newWeights)
        {
            int fromId = 0;

            for (int i = 0; i < layers.Count; i++)
            {
                fromId = layers[i].SetWeights(newWeights, fromId);
            }
        }

        public float[] GetWeights()
        {
            float[] weights = new float[totalWeightsCount];
            int id = 0;

            foreach (var neuron in layers)
            {
                float[] ws = neuron.GetWeights();

                for (int j = 0; j < ws.Length; j++)
                {
                    weights[id] = ws[j];
                    id++;
                }
            }

            return weights;
        }

        public float[] Synapse(float[] inputs)
        {
            float[] outputs = null;

            foreach (var neuron in layers)
            {
                outputs = neuron.Synapsis(inputs);
                inputs = outputs;
            }

            return outputs;
        }
    }
}
