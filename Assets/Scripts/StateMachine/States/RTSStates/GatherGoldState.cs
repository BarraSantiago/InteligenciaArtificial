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
            int food = Convert.ToInt32(parameters[1]);
            int gold = Convert.ToInt32(parameters[2]);
            int goldLimit = Convert.ToInt32(parameters[3]);
            Action OnMine = parameters[4] as Action;
            RTSNode<Vector2> currentNode = parameters[5] as RTSNode<Vector2>;

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
            BehaviourActions behaviours = new BehaviourActions();

            Action<RTSNode<Vector2>> onReachMine = parameters[0] as Action<RTSNode<Vector2>>;
            RTSNode<Vector2> currentNode = parameters[1] as RTSNode<Vector2>;

            behaviours.AddMultiThreadableBehaviours(0, () => { onReachMine?.Invoke(currentNode); });

            return behaviours;
        }

        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            Action<RTSNode<Vector2>> onLeaveMine = parameters[0] as Action<RTSNode<Vector2>>;
            RTSNode<Vector2> currentNode = parameters[1] as RTSNode<Vector2>;

            behaviours.AddMultiThreadableBehaviours(0, () => { onLeaveMine?.Invoke(currentNode); });

            return behaviours;
        }
    }
}