using System;
using Pathfinder;
using StateMachine.Agents.RTS;
using States;
using UnityEngine;

namespace StateMachine.States.RTSStates
{
    public class GatherGoldState : State
    {
        private Action<int> Onmine;
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            bool retreat = Convert.ToBoolean(parameters[0]);
            int? food = Convert.ToInt32(parameters[1]);
            int? gold = Convert.ToInt32(parameters[2]);
            int goldLimit = Convert.ToInt32(parameters[3]);
            Action OnMine = parameters[4] as Action;
            
            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                OnMine?.Invoke();
                
            });
            behaviours.AddMainThreadBehaviours(1, () => { Debug.Log("gold: " + gold); });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (retreat) OnFlag?.Invoke(RTSAgent.Flags.OnRetreat);
                if (food <= 0) OnFlag?.Invoke(RTSAgent.Flags.OnHunger);
                if (gold >= goldLimit) OnFlag?.Invoke(RTSAgent.Flags.OnFull);
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