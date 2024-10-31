using System;
using NeuralNetworkDirectory.ECS;
using NeuralNetworkDirectory.NeuralNet;
using Pathfinder;
using StateMachine.States.SimStates;
using UnityEngine;

namespace StateMachine.Agents.Simulation
{
    public class Carnivore : SimAgent
    {
        public Action OnAttack { get; set; }
        public override void Init()
        {
            base.Init();
            SimAgentType = SimAgentTypes.Carnivorous;
            foodTarget = SimNodeType.Corpse;
            FoodLimit = 1;
            movement = 2;
            
            OnAttack = () =>
            {
                SimAgent target = EcsPopulationManager.GetEntity(SimAgentTypes.Herbivore, CurrentNode);
                if (target == null) return;
                Herbivore herbivore = target as Herbivore;
                if (herbivore != null) herbivore.Hp--;
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
            Fsm.AddBehaviour<SimHuntState>(Behaviours.Eat, AttackEnterParameters);
            
            Fsm.AddBehaviour<SimAttackState>(Behaviours.Attack, AttackEnterParameters);
        }
        
        private object[] AttackEnterParameters()
        {
            object[] objects = { CurrentNode, OnAttack, output[0], output[1] };
            return objects;
        }

    }
}