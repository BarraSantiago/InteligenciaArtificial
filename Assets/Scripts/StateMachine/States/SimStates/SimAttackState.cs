using System;
using Pathfinder;
using StateMachine.Agents.Simulation;
using States;
using UnityEngine;

namespace StateMachine.States.SimStates
{
    public class SimAttackState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            var behaviours = new BehaviourActions();

            var onAttack = parameters[0] as Action;
            var outputBrain1 = (float[])parameters[1];
            var outputBrain2 = (float[])parameters[2];

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                onAttack?.Invoke();
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if(outputBrain1[0] > 0.5f) OnFlag?.Invoke(Flags.OnEat);
                if(outputBrain2[0] > 0.5f) OnFlag?.Invoke(Flags.OnAttack);
            });
            return behaviours;
        }
        
        public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
        {
            return default;
        }

        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            return default;
        }
    }
}