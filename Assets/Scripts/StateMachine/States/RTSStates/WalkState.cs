﻿using System;
using System.Collections.Generic;
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
            //List<Node<Vector2>> path = (List<Node<Vector2>>)parameters[4];


            behaviours.AddMainThreadBehaviours(0, () =>
            {
                //if (path.Count == 0) return;

                if (Vector2.Distance(currentNode.GetCoordinate(), targetNode.GetCoordinate()) < 0.2f)
                {
                    currentNode = targetNode;
                }
                
                currentNode.SetCoordinate( Vector2.Normalize(targetNode.GetCoordinate() - currentNode.GetCoordinate()) *
                               (speed * Time.deltaTime));
            });

            behaviours.SetTransitionBehaviour(() =>
            {

                if(retreat && targetNode.NodeType != NodeType.TownCenter)
                {
                    OnFlag?.Invoke(RTSAgent.Flags.OnRetreat);
                    return;
                }
                    
                if(targetNode.NodeType == NodeType.Mine && targetNode.gold <= 0)
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
            throw new System.NotImplementedException();

        }

        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            throw new System.NotImplementedException();
        }
    }
}