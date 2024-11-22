using System.Collections.Generic;
using System.Linq;
using NeuralNetworkDirectory.ECS;
using NeuralNetworkDirectory.NeuralNet;

namespace StateMachine.Agents.Simulation
{
    public static class InputCountCache
    {
        private static readonly Dictionary<(SimAgentTypes, BrainType), int> cache = new();

        public static int GetInputCount(SimAgentTypes agentType, BrainType brainType)
        {
            (SimAgentTypes agentType, BrainType brainType) key = (agentType, brainType);
            if (cache.TryGetValue(key, out int inputCount)) return inputCount;
            
            inputCount = EcsPopulationManager.inputCounts
                .FirstOrDefault(input => input.agentType == agentType && input.brainType == brainType).inputCount;
            cache[key] = inputCount;

            return inputCount;
        }
    }
}