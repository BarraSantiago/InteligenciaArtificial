using System;
using StateMachine.Agents.RTS;
using States;
using UnityEngine;

namespace StateMachine.States.RTSStates
{
    public class GetFoodState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();
            int? food = Convert.ToInt32(parameters[0]);
            int foodLimit = Convert.ToInt32(parameters[1]);

            behaviours.AddMultiThreadableBehaviours(0, () => { food++; });

            behaviours.AddMainThreadBehaviours(1, () => { Debug.Log("food: " + food); });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (food >= foodLimit) OnFlag?.Invoke(RTSAgent.Flags.OnFull);
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