using System;
using NeuralNetworkDirectory.ECS;
using NeuralNetworkDirectory.NeuralNet;
using Pathfinder;
using StateMachine.States.SimStates;
using UnityEngine.UIElements;
using Utils;

namespace StateMachine.Agents.Simulation
{
    public class Carnivore<TVector, TTransform> : SimAgent<TVector, TTransform>
        where TTransform : ITransform<IVector>
        where TVector : IVector, IEquatable<TVector>
    {
        public Action OnAttack { get; set; }

        public override void Init()
        {
            base.Init();
            agentType = SimAgentTypes.Carnivorous;
            foodTarget = SimNodeType.Corpse;
            FoodLimit = 1;
            movement = 2;

            brainTypes = new[] { BrainType.Movement, BrainType.Attack, BrainType.Eat };
            OnAttack = () =>
            {
                SimAgent<IVector, ITransform<IVector>> target =
                    EcsPopulationManager.GetEntity(SimAgentTypes.Herbivore, CurrentNode);
                if (target == null) return;
                if (target is Herbivore<TVector, TTransform> herbivore) herbivore.Hp--;
            };
        }

        protected override void ExtraInputs()
        {
            int brain = (int)BrainType.Attack;
            input[brain][0] = CurrentNode.GetCoordinate().X;
            input[brain][1] = CurrentNode.GetCoordinate().Y;
            SimAgent<IVector, ITransform<IVector>> target =
                EcsPopulationManager.GetNearestEntity(SimAgentTypes.Herbivore, CurrentNode);
            if (target == null)
            {
                input[brain][2] = NoTarget;
                input[brain][3] = NoTarget;
                return;
            }

            input[brain][2] = target.CurrentNode.GetCoordinate().X;
            input[brain][3] = target.CurrentNode.GetCoordinate().Y;
        }

        protected override void MovementInputs()
        {
            int brain = (int)BrainType.Movement;

            input[brain][0] = CurrentNode.GetCoordinate().X;
            input[brain][1] = CurrentNode.GetCoordinate().Y;
            SimAgent<IVector, ITransform<IVector>> target =
                EcsPopulationManager.GetNearestEntity(SimAgentTypes.Herbivore, CurrentNode);
            INode<IVector> nodeTarget = GetTarget(foodTarget);


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
        }

        protected override void ExtraBehaviours()
        {
            Fsm.AddBehaviour<SimEatCarnState>(Behaviours.Eat, EatTickParameters);

            Fsm.AddBehaviour<SimWalkCarnState>(Behaviours.Walk, AttackEnterParameters);

            Fsm.AddBehaviour<SimAttackState>(Behaviours.Attack, AttackEnterParameters);
        }

        private object[] AttackEnterParameters()
        {
            object[] objects = { CurrentNode, OnAttack, output[0], output[1] };
            return objects;
        }
    }
}