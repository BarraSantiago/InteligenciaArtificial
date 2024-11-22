using System.Collections.Generic;
using ECS.Patron;
using NeuralNetworkDirectory.NeuralNet;
using StateMachine.Agents.Simulation;

namespace NeuralNetworkDirectory.ECS
{
    public class OutputComponent : ECSComponent
    {
        public OutputComponent(SimAgentTypes agentType, Dictionary<int, BrainType> num)
        {
            this.outputsQty = 3;
            outputs = new float[outputsQty][];
            foreach (BrainType brain in num.Values)
            {
                EcsPopulationManager.NeuronInputCount inputsCount = EcsPopulationManager.InputCountCache[(brain, agentType)];
                outputs[GetBrainTypeKeyByValue(brain, num)] = new float[inputsCount.outputCount];
            }
        }

        public int GetBrainTypeKeyByValue(BrainType value, Dictionary<int, BrainType> brainTypes)
        {
            foreach (KeyValuePair<int, BrainType> kvp in brainTypes)
            {
                if (EqualityComparer<BrainType>.Default.Equals(kvp.Value, value))
                {
                    return kvp.Key;
                }
            }

            throw new KeyNotFoundException("The value is not present in the brainTypes dictionary.");
        }

        public int outputsQty;
        public float[][] outputs;
    }
}