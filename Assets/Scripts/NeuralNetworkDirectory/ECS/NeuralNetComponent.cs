using ECS.Patron;
using System.Collections.Generic;
using NeuralNetworkDirectory.NeuralNet;
using StateMachine.Agents.Simulation;

namespace NeuralNetworkDirectory.ECS
{
    public class NeuralNetComponent : ECSComponent
    {
        public float[] Fitness;
        public float[] FitnessMod;
        public List<List<NeuronLayer>> Layers { get; set; } = new();
        
        public void SetWeights(int index, float[] newWeights)
        {
            var fromId = 0;

            for (var i = 0; i < Layers[index].Count; i++) fromId = Layers[index][i].SetWeights(newWeights, fromId);
        }
        
        public void Reward(float reward, BrainType brainType)
        {
            int id = EcsPopulationManager.GetBrainTypeKeyByValue(brainType, Layers[0][0].AgentType);
            FitnessMod[id] = IncreaseFitnessMod(FitnessMod[id]);
            Fitness[id] += reward * FitnessMod[id];
        }
        
        public void Punish(float punishment, BrainType brainType)
        {
            int id = EcsPopulationManager.GetBrainTypeKeyByValue(brainType, Layers[0][0].AgentType);

            FitnessMod[id] = DecreaseFitnessMod(FitnessMod[id]);
            Fitness[id] /= punishment + 0.05f * FitnessMod[id];
        }
        
        public float IncreaseFitnessMod(float fitnessMod)
        {
            const float MAX_FITNESS = 2;
            const float MOD = 1.1f;
            fitnessMod *= MOD;
            if (fitnessMod > MAX_FITNESS) fitnessMod = MAX_FITNESS;
            return fitnessMod;
        }

        public float DecreaseFitnessMod(float fitnessMod)
        {
            const float MOD = 0.9f;
            return fitnessMod * MOD;
        }
    }
}