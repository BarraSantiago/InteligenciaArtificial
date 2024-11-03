using NeuralNetworkDirectory.ECS;
using NeuralNetworkDirectory.NeuralNet;
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
            agentType = SimAgentTypes.Herbivore;
            foodTarget = SimNodeType.Bush;
            brainTypes = new[] {BrainType.Movement, BrainType.Escape, BrainType.Eat};

            hp = InitialHp;
        }

        protected override void ExtraInputs()
        {
            int brain = (int)BrainType.Attack;
            input[brain][0] = CurrentNode.GetCoordinate().x;
            input[brain][1] = CurrentNode.GetCoordinate().y;
            SimAgent target = EcsPopulationManager.GetNearestEntity(SimAgentTypes.Carnivorous, CurrentNode);
            input[brain][2] = target.CurrentNode.GetCoordinate().x;
            input[brain][3] = target.CurrentNode.GetCoordinate().y;
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
            input[brain][7] = Hp;

        }

        private void Die()
        {
            var node = EcsPopulationManager.CoordinateToNode(CurrentNode);
            node.NodeType = SimNodeType.Corpse;
            node.food = FoodDropped;
        }

        protected override void ExtraBehaviours()
        {
            Fsm.AddBehaviour<SimEatHerbState>(Behaviours.Eat, EatTickParameters);

            Fsm.AddBehaviour<SimWalkHerbState>(Behaviours.Escape, WalkTickParameters);
        }
    }
}