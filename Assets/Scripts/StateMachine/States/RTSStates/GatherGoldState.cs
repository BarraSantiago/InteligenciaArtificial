using System;
using StateMachine.Agents.RTS;
using States;
using UnityEngine;

namespace StateMachine.States.RTSStates
{
    public class GatherGoldState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            bool retreat = Convert.ToBoolean(parameters[0]);
            refInt food = parameters[1] as refInt;
            refInt gold = parameters[2] as refInt;
            refInt lastTimeEat = parameters[3] as refInt;
            int goldPerFood = Convert.ToInt32(parameters[4]);
            int goldLimit = Convert.ToInt32(parameters[5]);

            behaviours.AddMainThreadBehaviours(0, () =>
            {
                if (food.value <= 0) return;
                
                Debug.Log("gold: " + gold.value);
                
                gold.value++;
                lastTimeEat.value++;

                if (lastTimeEat.value < goldPerFood) return;

                food.value--;
                lastTimeEat.value = 0;
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (retreat) OnFlag?.Invoke(RTSAgent.Flags.OnRetreat);
                if (food.value <= 0) OnFlag?.Invoke(RTSAgent.Flags.OnHunger);
                if (gold.value >= goldLimit) OnFlag?.Invoke(RTSAgent.Flags.OnFull);
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