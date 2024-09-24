using System;
using Pathfinder;
using StateMachine.Agents.RTS;
using States;
using UnityEngine;

namespace StateMachine.States.RTSStates
{
    public class DeliverFoodState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();
            int? food = Convert.ToInt32(parameters[0]);
            Node<Vector2> node = parameters[1] as Node<Vector2>;

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                if (food <= 0) return;

                food--;
                node.food++;
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (food <= 0) OnFlag?.Invoke(RTSAgent.Flags.OnHunger);
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