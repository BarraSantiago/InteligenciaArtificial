using System;
using Pathfinder;
using States;
using UnityEngine;

namespace StateMachine.States.SimStates
{
    public class SimWalkState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            var behaviours = new BehaviourActions();

            var currentNode = parameters[0] as SimNode<Vector2>;
            var targetNode = parameters[1] as RTSNode<Vector2>;
            var position = (Transform)parameters[2];
            var onMove = parameters[3] as Action;

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                onMove?.Invoke();
                
            });

            behaviours.AddMainThreadBehaviours(1, () =>
            {
                if (currentNode == null) return;

                position.position = new Vector3(currentNode.GetCoordinate().x, currentNode.GetCoordinate().y);
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                /*
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

                switch (currentNode.NodeType)
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
                */
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