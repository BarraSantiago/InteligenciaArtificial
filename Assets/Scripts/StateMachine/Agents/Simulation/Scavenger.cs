using NeuralNetworkDirectory.ECS;
using NeuralNetworkDirectory.NeuralNet;
using Pathfinder;
using UnityEngine;

namespace StateMachine.Agents.Simulation
{
    public class Scavenger : SimAgent
    {
        public override void Init()
        {
            base.Init();
            SimAgentType = SimAgentTypes.Scavenger;
            foodTarget = SimNodeType.Carrion;
            FoodLimit = 20;
            movement = 5;
        }
        
        protected override void MovementInputs()
        {
            int brain = (int)BrainType.Movement;
            
            input[brain][0] = CurrentNode.GetCoordinate().x;
            input[brain][1] = CurrentNode.GetCoordinate().y;
            SimAgent target = EcsPopulationManager.GetNearestEntity(SimAgentTypes.Carnivorous, CurrentNode);
            input[brain][2] = target.CurrentNode.GetCoordinate().x;
            input[brain][3] = target.CurrentNode.GetCoordinate().y;
            SimNode<Vector2> nodeTarget = GetTarget(foodTarget);
            input[brain][4] = nodeTarget.GetCoordinate().x;
            input[brain][5] = nodeTarget.GetCoordinate().y;
            input[brain][6] = Food;

        }
    }
}