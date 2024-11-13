using System;
using NeuralNetworkDirectory.ECS;
using NeuralNetworkDirectory.NeuralNet;
using Pathfinder;
using StateMachine.States.SimStates;
using Utils;

namespace StateMachine.Agents.Simulation
{
    public class Herbivore<TVector, TTransform> : SimAgent<TVector, TTransform>
        where TTransform : ITransform<IVector>, new()
        where TVector : IVector, IEquatable<TVector>
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
            CalculateInputs();
            brainTypes = new[] { BrainType.Movement, BrainType.Escape, BrainType.Eat };

            hp = InitialHp;
        }

        protected override void ExtraInputs()
        {
            int brain = (int)BrainType.Escape;
            input[brain][0] = CurrentNode.GetCoordinate().X;
            input[brain][1] = CurrentNode.GetCoordinate().Y;
            var target = EcsPopulationManager.GetNearestEntity(SimAgentTypes.Carnivorous, CurrentNode);
            if (target == null)
            {
                input[brain][2] = NoTarget;
                input[brain][3] = NoTarget;
            }
            else
            {
                input[brain][2] = target.CurrentNode.GetCoordinate().X;
                input[brain][3] = target.CurrentNode.GetCoordinate().Y;
            }
        }

        protected override void MovementInputs()
        {
            int brain = (int)BrainType.Movement;

            input[brain][0] = CurrentNode.GetCoordinate().X;
            input[brain][1] = CurrentNode.GetCoordinate().Y;

            var target = EcsPopulationManager.GetNearestEntity(SimAgentTypes.Carnivorous, CurrentNode);
            if (target == null)
            {
                input[brain][2] = NoTarget;
                input[brain][3] = NoTarget;
            }
            else
            {
                input[brain][2] = target.CurrentNode.GetCoordinate().X;
                input[brain][3] = target.CurrentNode.GetCoordinate().Y;
            }

            var nodeTarget = GetTarget(foodTarget);
            if (nodeTarget == null)
            {
                input[brain][4] = NoTarget;
                input[brain][5] = NoTarget;
            }
            else
            {
                input[brain][4] = nodeTarget.GetCoordinate().X;
                input[brain][5] = nodeTarget.GetCoordinate().Y;
            }

            input[brain][6] = Food;
            input[brain][7] = Hp;
        }

        private void Die()
        {
            var node = CurrentNode;
            node.NodeType = SimNodeType.Corpse;
            node.Food = FoodDropped;
        }

        protected override void ExtraBehaviours()
        {
            Fsm.AddBehaviour<SimEatHerbState>(Behaviours.Eat, EatTickParameters);

            Fsm.AddBehaviour<SimWalkHerbState>(Behaviours.Escape, WalkTickParameters);
        }
    }
}