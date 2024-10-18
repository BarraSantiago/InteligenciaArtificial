using System;
using StateMachine.Agents.RTS;
using States;

namespace StateMachine.States.RTSStates
{
    public class DeliverFoodState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            var behaviours = new BehaviourActions();
            var food = Convert.ToInt32(parameters[0]);
            var onDeliverFood = parameters[1] as Action;
            var retreat = Convert.ToBoolean(parameters[2]);

            behaviours.AddMultiThreadableBehaviours(0, () => { onDeliverFood?.Invoke(); });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (food <= 0) OnFlag?.Invoke(RTSAgent.Flags.OnHunger);
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