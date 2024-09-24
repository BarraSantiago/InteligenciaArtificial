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
            int food = Convert.ToInt32(parameters[0]);
            int foodLimit = Convert.ToInt32(parameters[1]);
            Action onGatherFood = parameters[2] as Action;
            bool retreat = Convert.ToBoolean(parameters[3]);

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                onGatherFood?.Invoke();
            });

            behaviours.AddMainThreadBehaviours(1, () =>
            {
                Debug.Log("food: " + onGatherFood);
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (food >= foodLimit) OnFlag?.Invoke(RTSAgent.Flags.OnFull);
                if (retreat) OnFlag?.Invoke(RTSAgent.Flags.OnRetreat);
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