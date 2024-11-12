using System;
using ECS.Patron;
using NeuralNetworkDirectory.NeuralNet;

namespace NeuralNetworkDirectory.ECS
{
    public class OutputComponent : ECSComponent
    {
        public OutputComponent(int _outputsQty)
        {
            this.outputsQty = Enum.GetValues(typeof(BrainType)).Length;
            outputs = new float[outputsQty][];
            for (var i = 0; i < outputsQty; i++)
            {
                outputs[i] = new float[_outputsQty];
            }
        }

        public int outputsQty;
        public float[][] outputs;
    }
}