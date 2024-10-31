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

            var currentNode = parameters[0] as SimNode<Vector2>;
            var onAttack = parameters[1] as Action;
            var outputBrain1 = (float[])parameters[2];
            var outputBrain2 = (float[])parameters[3];

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                onAttack?.Invoke();
                
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if(outputBrain1[0] > 0.5f) OnFlag?.Invoke(SimAgent.Flags.OnEat);
                if(outputBrain1[1] > 0.5f) OnFlag?.Invoke(SimAgent.Flags.OnSearchFood);
                if(outputBrain2[0] > 0.5f) OnFlag?.Invoke(SimAgent.Flags.OnAttack);
                
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