using NeuralNetworkDirectory.ECS;
using Pathfinder;
using UnityEngine;

namespace StateMachine.Agents.Simulation
{
    public class Carnivore : SimAgent
    {
        public override void Init()
        {
            base.Init();
            FoodLimit = 1;
            foodTarget = SimNodeType.Corpse;
            movement = 2;
            SimAgentType = SimAgentTypes.Carnivorous;
        }
        
        protected override void AttackInputs()
        {
            input[1][0] = CurrentNode.GetCoordinate().x;
            input[1][1] = CurrentNode.GetCoordinate().y;
            SimNode<Vector2> target = EcsPopulationManager.GetEntity(SimAgentTypes.Herbivore, CurrentNode);
            input[1][2] = target.GetCoordinate().x;
            input[1][3] = target.GetCoordinate().y;
        }
    }
}