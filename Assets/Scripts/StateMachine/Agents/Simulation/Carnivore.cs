using System;
using NeuralNetworkDirectory.ECS;
using NeuralNetworkDirectory.NeuralNet;
using Pathfinder;
using StateMachine.States.SimStates;
using UnityEngine.UIElements;
using Utils;

namespace StateMachine.Agents.Simulation
{
    public class Carnivore<TVector, TTransform> : SimAgent<TVector,TTransform> 
        where TTransform : ITransform<TVector>
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
            
            brainTypes = new[] {BrainType.Movement, BrainType.Attack, BrainType.Eat};
            OnAttack = () =>
            {
                SimAgent<TVector, TTransform> target = EcsPopulationManager.GetEntity(SimAgentTypes.Herbivore, CurrentNode);
                if (target == null) return;
                if (target is Herbivore<TVector, TTransform> herbivore) herbivore.Hp--;
            };
        }
        
        protected override void ExtraInputs()
        {
            int brain = (int)BrainType.Attack;
            input[brain][0] = CurrentNode.GetCoordinate().x;
            input[brain][1] = CurrentNode.GetCoordinate().y;
            SimAgent target = EcsPopulationManager.GetNearestEntity(SimAgentTypes.Herbivore, CurrentNode);
            input[brain][2] = target.CurrentNode.GetCoordinate().x;
            input[brain][3] = target.CurrentNode.GetCoordinate().y;
        }

        protected override void MovementInputs()
        {
            int brain = (int)BrainType.Movement;
            
            input[brain][0] = CurrentNode.GetCoordinate().x;
            input[brain][1] = CurrentNode.GetCoordinate().y;
            SimAgent target = EcsPopulationManager.GetNearestEntity(SimAgentTypes.Herbivore, CurrentNode);
            input[brain][2] = target.CurrentNode.GetCoordinate().x;
            input[brain][3] = target.CurrentNode.GetCoordinate().y;
            SimNode<Vector2> nodeTarget = GetTarget(foodTarget);
            input[brain][4] = nodeTarget.GetCoordinate().x;
            input[brain][5] = nodeTarget.GetCoordinate().y;
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