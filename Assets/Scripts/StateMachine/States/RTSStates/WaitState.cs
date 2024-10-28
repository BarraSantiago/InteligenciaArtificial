using System;
using Pathfinder;
using StateMachine.Agents.RTS;
using States;
using UnityEngine;

namespace StateMachine.States.RTSStates
{
    public class WaitState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            var behaviours = new BehaviourActions();

            var retreat = (bool)parameters[0];
            int? food = Convert.ToInt32(parameters[1]);
            int? gold = Convert.ToInt32(parameters[2]);
            var currentNode = (RTSNode<Vector2>)parameters[3];
            var OnWait = parameters[4] as Action;


            behaviours.AddMultiThreadableBehaviours(0, () => { OnWait?.Invoke(); });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (retreat)
                {
                    if (currentNode.RtsNodeType != RTSNodeType.TownCenter) OnFlag?.Invoke(RTSAgent.Flags.OnRetreat);
                    return;
                }

                if (currentNode.RtsNodeType == RTSNodeType.Empty ||
                    (currentNode.RtsNodeType == RTSNodeType.Mine && currentNode.gold <= 0))
                {
                    OnFlag?.Invoke(RTSAgent.Flags.OnTargetLost);
                    return;
                }

                if (food > 0 && currentNode.RtsNodeType == RTSNodeType.Mine)
                {
                    OnFlag?.Invoke(RTSAgent.Flags.OnGather);
                    return;
                }

                if (gold <= 0 && currentNode.RtsNodeType == RTSNodeType.TownCenter) OnFlag?.Invoke(RTSAgent.Flags.OnGather);
            });

            return behaviours;
        }

        public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
        {
            var behaviours = new BehaviourActions();

            var currentNode = parameters[0] as RTSNode<Vector2>;
            var onReachMine = parameters[1] as Action<RTSNode<Vector2>>;

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                if (currentNode.RtsNodeType == RTSNodeType.Mine) onReachMine?.Invoke(currentNode);
            });

            return behaviours;
        }

        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            var behaviours = new BehaviourActions();

            if (parameters == null) return default;
            var currentNode = parameters[0] as RTSNode<Vector2>;
            var onLeaveMine = parameters[1] as Action<RTSNode<Vector2>>;

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                if (currentNode.RtsNodeType == RTSNodeType.Mine) onLeaveMine?.Invoke(currentNode);
            });

            return behaviours;
        }
    }
}