using StateMachine.Agents.RTS;
using States;

namespace StateMachine.States.RTSStates
{
    public class GatherGoldState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            bool retreat = (bool) parameters[0];
            int food = (int) parameters[1];
            int gold = (int) parameters[2];
            int lastTimeEat = (int) parameters[3];
            int goldPerFood = (int) parameters[4];
            int goldLimit = (int) parameters[5];
            
            behaviours.AddMainThreadBehaviours(0, () =>
            {
                if(food <= 0) return;
                
                gold++;
                lastTimeEat++;

                if (lastTimeEat < goldPerFood) return;
                
                food--;
                lastTimeEat = 0;
            });
            
            behaviours.SetTransitionBehaviour(() =>
            {
                if(retreat) OnFlag?.Invoke(RTSAgent.Flags.OnRetreat);
                if(food <= 0) OnFlag?.Invoke(RTSAgent.Flags.OnHunger);
                if(gold >= goldLimit) OnFlag?.Invoke(RTSAgent.Flags.OnFull);
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