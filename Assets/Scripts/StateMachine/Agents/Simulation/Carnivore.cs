using NeuralNetworkDirectory.ECS;
using Pathfinder;
using StateMachine.States.SimStates;
using UnityEngine;

namespace StateMachine.Agents.Simulation
{
    public class Carnivore : SimAgent
    {
        public override void Init()
        {
            base.Init();
            SimAgentType = SimAgentTypes.Carnivorous;
            foodTarget = SimNodeType.Corpse;
            FoodLimit = 1;
            movement = 2;
        }
        
        protected override void ExtraInputs()
        {
            input[1][0] = CurrentNode.GetCoordinate().x;
            input[1][1] = CurrentNode.GetCoordinate().y;
            SimNode<Vector2> target = EcsPopulationManager.GetEntity(SimAgentTypes.Herbivore, CurrentNode);
            input[1][2] = target.GetCoordinate().x;
            input[1][3] = target.GetCoordinate().y;
        }

        protected override void ExtraBehaviours()
        {
            Fsm.AddBehaviour<SimHuntState>(Behaviours.Eat, AttackEnterParameters);
        }
        
        private object[] AttackEnterParameters()
        {
            object[] objects = { };
            return objects;
        }

    }
}