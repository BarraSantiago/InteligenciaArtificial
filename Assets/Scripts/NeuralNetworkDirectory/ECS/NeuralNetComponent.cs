using ECS.Patron;
using System.Collections.Generic;
using NeuralNetworkDirectory.NeuralNet;

namespace NeuralNetworkDirectory.ECS
{
    public class NeuralNetComponent : ECSComponent
    {
        public float[] Fitness;
        public float[] FitnessMod;
        public List<List<NeuronLayer>> Layers { get; set; } = new();
    }
}