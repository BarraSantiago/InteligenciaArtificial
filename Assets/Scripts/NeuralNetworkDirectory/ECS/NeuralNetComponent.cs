using ECS.Patron;
using System.Collections.Generic;
using NeuralNetworkDirectory.NeuralNet;

namespace NeuralNetworkDirectory.ECS
{
    public class NeuralNetComponent : ECSComponent
    {
        public List<List<NeuronLayer>> Layers { get; set; } = new();
    }
}