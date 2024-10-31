using System;
using NeuralNetworkDirectory.ECS;
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
            input[1][0] = CurrentNode.GetCoordinate().x;
            input[1][1] = CurrentNode.GetCoordinate().y;
            SimAgent target = EcsPopulationManager.GetNearestEntity(SimAgentTypes.Herbivore, CurrentNode);
            input[1][2] = target.CurrentNode.GetCoordinate().x;
            input[1][3] = target.CurrentNode.GetCoordinate().y;
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