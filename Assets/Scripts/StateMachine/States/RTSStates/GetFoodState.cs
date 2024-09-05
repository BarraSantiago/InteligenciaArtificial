using System;
using StateMachine.Agents.RTS;
using States;

namespace StateMachine.States.RTSStates
{
    public class GetFoodState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();
            refInt food = parameters[0] as refInt;
            int foodLimit = Convert.ToInt32(parameters[1]);

            behaviours.AddMainThreadBehaviours(0, () =>
            {
                food.value++;
            });
            behaviours.SetTransitionBehaviour(() =>
            {
                if(food.value >= foodLimit) OnFlag?.Invoke(RTSAgent.Flags.OnFull);
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