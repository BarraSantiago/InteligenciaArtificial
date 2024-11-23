using System.Collections.Generic;
using UnityEngine;

namespace NeuralNetworkDirectory.NeuralNet
{
    public class NeuralNetwork
    {
        private readonly List<NeuronLayer> layers = new();
        private int totalWeightsCount;

        public int InputsCount { get; private set; }

        public bool AddNeuronLayer(int neuronsCount, float bias, float p)
        {
            if (layers.Count == 0)
            {
                Debug.LogError("Call AddFirstNeuronLayer(int inputsCount, float bias, float p) for the first layer.");
                return false;
            }

            return AddNeuronLayer((int)layers[^1].OutputsCount, neuronsCount, bias, p);
        }

        public bool AddFirstNeuronLayer(int inputsCount, float bias, float p)
        {
            if (layers.Count != 0)
            {
                Debug.LogError("Call AddNeuronLayer(int neuronCount, float bias, float p) for the rest of the layers.");
                return false;
            }

            InputsCount = inputsCount;

            return AddNeuronLayer(inputsCount, inputsCount, bias, p);
        }

        private bool AddNeuronLayer(int inputsCount, int neuronsCount, float bias, float p)
        {
            if (layers.Count > 0 && layers[^1].OutputsCount != inputsCount)
            {
                Debug.LogError("Inputs Count must match outputs from previous layer.");
                return false;
            }

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

            //for (int i = 0; i < layers.Count; i++) fromId = layers[i].SetWeights(newWeights, fromId);
        }

        public float[] Synapsis(float[] inputs)
        {
            float[] outputs = null;

            for (int i = 0; i < layers.Count; i++)
            {
                //outputs = layers[i].Synapsis(inputs);
                inputs = outputs;
            }

            return outputs;
        }
    }
}