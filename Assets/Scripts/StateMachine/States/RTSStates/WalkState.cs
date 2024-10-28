using System;
using System.Collections.Generic;
using Pathfinder;
using StateMachine.Agents.RTS;
using States;
using UnityEngine;

namespace StateMachine.States.RTSStates
{
    public class WalkState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            var behaviours = new BehaviourActions();

            var currentNode = parameters[0] as RTSNode<Vector2>;
            var targetNode = parameters[1] as RTSNode<Vector2>;
            var retreat = (bool)parameters[2];
            var position = (Transform)parameters[3];
            var onMove = parameters[4] as Action;

            behaviours.AddMultiThreadableBehaviours(0, () => { onMove?.Invoke(); });

            behaviours.AddMainThreadBehaviours(1, () =>
            {
                if (currentNode == null) return;

                position.position = new Vector3(currentNode.GetCoordinate().x, currentNode.GetCoordinate().y);
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (retreat && (targetNode is null || targetNode.RtsNodeType != RTSNodeType.TownCenter))
                {
                    OnFlag?.Invoke(RTSAgent.Flags.OnRetreat);
                    return;
                }


                if (currentNode == null || targetNode == null ||
                    (targetNode.RtsNodeType == RTSNodeType.Mine && targetNode.gold <= 0))
                {
                    OnFlag?.Invoke(RTSAgent.Flags.OnTargetLost);
                    return;
                }

                if (currentNode.GetCoordinate() == targetNode.GetCoordinate()) return;

                //if ((currentNode.GetCoordinate()).Equals(targetNode.GetCoordinate())) return;

                switch (currentNode.RtsNodeType)
                {
                    case RTSNodeType.Mine:
                        OnFlag?.Invoke(RTSAgent.Flags.OnGather);
                        break;
                    case RTSNodeType.TownCenter:
                        OnFlag?.Invoke(RTSAgent.Flags.OnWait);
                        break;
                    case RTSNodeType.Empty:
                    case RTSNodeType.Blocked:
                    default:
                        OnFlag?.Invoke(RTSAgent.Flags.OnTargetLost);
                        break;
                }
            });

            return behaviours;
        }

        public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
        {
            var behaviours = new BehaviourActions();

            var currentNode = parameters[0] as RTSNode<Vector2>;
            var targetNode = parameters[1] as RTSNode<Vector2>;
            var path = (List<RTSNode<Vector2>>)parameters[2];
            var pathfinder =
                parameters[3] as Pathfinder<RTSNode<Vector2>, Vector2, NodeVoronoi>;
            var type = (RTSAgent.AgentTypes)parameters[4];

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                if (currentNode != null && targetNode != null)
                    path = pathfinder.FindPath(currentNode, targetNode);
            });

            return behaviours;
        }

        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            return default;
        }
    }
}