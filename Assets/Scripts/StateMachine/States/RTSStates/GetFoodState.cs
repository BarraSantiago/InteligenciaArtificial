using StateMachine.Agents.RTS;
using States;

namespace StateMachine.States.RTSStates
{
    public class GetFoodState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            throw new System.NotImplementedException();
        }

        public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();
            int food = (int) parameters[0];
            int foodLimit = (int) parameters[1];
            
            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                food += foodLimit;
            });
            behaviours.SetTransitionBehaviour(() =>
            {
                if(food >= foodLimit) OnFlag?.Invoke(RTSAgent.Flags.OnFull);
            });
            return behaviours;
        }

        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            throw new System.NotImplementedException();
        }
    }
}