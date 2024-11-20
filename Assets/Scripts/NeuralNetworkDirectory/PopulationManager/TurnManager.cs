using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECS.Patron;
using Flocking;
using NeuralNetworkDirectory.ECS;
using NeuralNetworkDirectory.NeuralNet;
using StateMachine.Agents.Simulation;
using Utils;

namespace NeuralNetworkDirectory.PopulationManager
{
    using SimAgentType = SimAgent<IVector, ITransform<IVector>>;
    using SimBoid = Boid<IVector, ITransform<IVector>>;

    public class TurnManager
    {
        public void UpdateInputs(Dictionary<uint, SimAgentType> _agents)
        {
            Parallel.ForEach(_agents.Values, entity =>
            {
                entity.UpdateInputs();
            });
            
            Parallel.ForEach(_agents, entity =>
            {
                var inputComponent = ECSManager.GetComponent<InputComponent>(entity.Key);
                if (inputComponent != null && _agents.ContainsKey(entity.Key))
                {
                    inputComponent.inputs = _agents[entity.Key].input;
                }
            });
        }

        public void UpdateOutputs(Dictionary<uint, SimAgentType> _agents, Dictionary<uint, Scavenger<IVector, ITransform<IVector>>> _scavengers)
        {
            Parallel.ForEach(_agents, entity =>
            {
                var outputComponent = ECSManager.GetComponent<OutputComponent>(entity.Key);
                if (outputComponent == null || !_agents.ContainsKey(entity.Key)) return;

                _agents[entity.Key].output = outputComponent.outputs;
            });

            Parallel.ForEach(_scavengers, entity =>
            {
                var outputComponent = ECSManager.GetComponent<OutputComponent>(entity.Key);
                var boid = _scavengers[entity.Key]?.boid;

                if (boid != null && outputComponent != null)
                {
                    UpdateBoidOffsets(boid, outputComponent.outputs
                        [DataContainer.GetBrainTypeKeyByValue(BrainType.Flocking, SimAgentTypes.Scavenger)]);
                }
            });  
        }
        
        public void AgentsTick(Dictionary<uint, SimAgentType> _agents, int behaviourCount)
        {
            for (int i = 0; i < behaviourCount; i++)
            {
                var tasks = _agents.Select(entity => Task.Run(() => entity.Value.Fsm.MultiThreadTick(i)))
                    .ToArray();

                foreach (var entity in _agents)
                {
                    entity.Value.Fsm.MainThreadTick(i);
                }

                Task.WaitAll(tasks);
            }
        }
        
        private void UpdateBoidOffsets(SimBoid boid, float[] outputs)
        {
            boid.cohesionOffset = outputs[0];
            boid.separationOffset = outputs[1];
            boid.directionOffset = outputs[2];
            boid.alignmentOffset = outputs[3];
        }

    }
}