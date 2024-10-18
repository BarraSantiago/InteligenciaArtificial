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
            var behaviours = new BehaviourActions();

            var retreat = Convert.ToBoolean(parameters[0]);
            var food = Convert.ToInt32(parameters[1]);
            var gold = Convert.ToInt32(parameters[2]);
            var goldLimit = Convert.ToInt32(parameters[3]);
            var OnMine = parameters[4] as Action;
            var currentNode = parameters[5] as Node<Vector2>;

            behaviours.AddMultiThreadableBehaviours(0, () => { OnMine?.Invoke(); });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (retreat) OnFlag?.Invoke(RTSAgent.Flags.OnRetreat);
                if (food <= 0) OnFlag?.Invoke(RTSAgent.Flags.OnHunger);
                if (gold >= goldLimit) OnFlag?.Invoke(RTSAgent.Flags.OnFull);
                if (currentNode.gold <= 0) OnFlag?.Invoke(RTSAgent.Flags.OnTargetLost);
            });

            return behaviours;
        }

        public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
        {
            var behaviours = new BehaviourActions();

            var onReachMine = parameters[0] as Action<Node<Vector2>>;
            var currentNode = parameters[1] as Node<Vector2>;

            behaviours.AddMultiThreadableBehaviours(0, () => { onReachMine?.Invoke(currentNode); });

            return behaviours;
        }

        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            var behaviours = new BehaviourActions();

            var onLeaveMine = parameters[0] as Action<Node<Vector2>>;
            var currentNode = parameters[1] as Node<Vector2>;

            behaviours.AddMultiThreadableBehaviours(0, () => { onLeaveMine?.Invoke(currentNode); });

            return behaviours;
        }
    }
}