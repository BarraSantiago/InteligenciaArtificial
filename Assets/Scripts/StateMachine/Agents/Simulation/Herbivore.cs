using NeuralNetworkDirectory.ECS;
using Pathfinder;
using StateMachine.States.SimStates;
using UnityEngine;

namespace StateMachine.Agents.Simulation
{
    public class Herbivore : SimAgent
    {
        public int Hp
        {
            get => hp;
            set
            {
                hp = value;
                if (hp <= 0) Die();
            }
        }

        private int hp;
        private const int FoodDropped = 1;
        private const int InitialHp = 2;

        public override void Init()
        {
            base.Init();
            SimAgentType = SimAgentTypes.Herbivore;
            foodTarget = SimNodeType.Bush;
            hp = InitialHp;
        }

        protected override void ExtraInputs()
        {
            input[1][0] = CurrentNode.GetCoordinate().x;
            input[1][1] = CurrentNode.GetCoordinate().y;
            SimAgent target = EcsPopulationManager.GetNearestEntity(SimAgentTypes.Carnivorous, CurrentNode);
            input[1][2] = target.CurrentNode.GetCoordinate().x;
            input[1][3] = target.CurrentNode.GetCoordinate().y;
        }

        private void Die()
        {
            CurrentNode.NodeType = SimNodeType.Corpse;
            CurrentNode.food = FoodDropped;
        }

        protected override void ExtraBehaviours()
        {
            Fsm.AddBehaviour<SimEscapeState>(Behaviours.Escape, WalkTickParameters);
        }
    }
}