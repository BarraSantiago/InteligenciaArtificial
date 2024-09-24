using System;
using Pathfinder;
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
            int? food = Convert.ToInt32(parameters[1]);
            int? gold = Convert.ToInt32(parameters[2]);
            int? lastTimeEat = Convert.ToInt32(parameters[3]);
            int goldPerFood = Convert.ToInt32(parameters[4]);
            int goldLimit = Convert.ToInt32(parameters[5]);
            Node<Vector2> mine = parameters[6] as Node<Vector2>;

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                if (food <= 0) return;

                gold++;
                lastTimeEat++;
                mine.gold--;

                if (lastTimeEat < goldPerFood) return;

                food--;
                lastTimeEat = 0;

                if (food > 0 || mine.food <= 0) return;

                food++;
                mine.food--;
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