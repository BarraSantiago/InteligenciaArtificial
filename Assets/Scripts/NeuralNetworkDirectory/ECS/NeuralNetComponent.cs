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
        
        public void SetWeights(int index, float[] newWeights)
        {
            var fromId = 0;

            for (var i = 0; i < Layers[index].Count; i++) fromId = Layers[index][i].SetWeights(newWeights, fromId);
        }
        
        public void Reward(float reward, BrainType brainType)
        { 
            FitnessMod[(int)brainType] = IncreaseFitnessMod(FitnessMod[(int)brainType]);
            Fitness[(int)brainType] += reward * FitnessMod[(int)brainType];
        }
        
        public void Punish(float punishment, BrainType brainType)
        {
            FitnessMod[(int)brainType] = DecreaseFitnessMod(FitnessMod[(int)brainType]);
            Fitness[(int)brainType] /= punishment + 0.05f * FitnessMod[(int)brainType];
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
            return fitnessMod *= MOD;
        }
    }
}