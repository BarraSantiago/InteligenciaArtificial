using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Pathfinder;
using StateMachine.Agents.RTS;
using States;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace StateMachine.States.RTSStates
{
    public class WalkState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            Node<Vector2> currentNode = (Node<Vector2>)parameters[0];
            Node<Vector2> targetNode = (Node<Vector2>)parameters[1];
            float speed = Convert.ToSingle(parameters[2]);
            bool retreat = (bool)parameters[3];
            Transform position = (Transform)parameters[4];
            List<Node<Vector2>> path = (List<Node<Vector2>>)parameters[5];


            behaviours.AddMainThreadBehaviours(0, () =>
            {
                //if (path.Count == 0) return;

                if (Vector2.Distance(currentNode.GetCoordinate(), targetNode.GetCoordinate()) < 0.5f)
                {
                    currentNode = targetNode;
                    return;
                }

                currentNode = path[0];
                path.RemoveAt(0);
                
                position.position = new Vector3(currentNode.GetCoordinate().X, currentNode.GetCoordinate().Y);
                
                //Vector3 direction = new Vector3(targetNode.GetCoordinate().X - currentNode.GetCoordinate().X,targetNode.GetCoordinate().Y - currentNode.GetCoordinate().Y).normalized;
                //position.position += direction * (speed * Time.deltaTime);
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (retreat && targetNode.NodeType != NodeType.TownCenter)
                {
                    OnFlag?.Invoke(RTSAgent.Flags.OnRetreat);
                    return;
                }

                if (targetNode.NodeType == NodeType.Mine && targetNode.gold <= 0)
                {
                    OnFlag?.Invoke(RTSAgent.Flags.OnTargetLost);
                    return;
                }

                if (!currentNode.Equals(targetNode)) return;

                switch (targetNode.NodeType)
                {
                    case NodeType.Mine:
                        OnFlag?.Invoke(RTSAgent.Flags.OnGather);
                        break;
                    case NodeType.TownCenter:
                        OnFlag?.Invoke(RTSAgent.Flags.OnWait);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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