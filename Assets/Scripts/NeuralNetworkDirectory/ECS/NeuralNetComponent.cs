using ECS.Patron;
using System.Collections.Generic;
using NeuralNetworkDirectory.NeuralNet;

namespace NeuralNetworkDirectory.ECS
{
    public class NeuralNetComponent : ECSComponent
    {
        public List<NeuronLayer> Layers { get; set; } = new();
        public int TotalWeightsCount { get; set; }
        public int InputsCount { get; set; }
    }
}