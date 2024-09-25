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
            BehaviourActions behaviours = new BehaviourActions();

            bool retreat = (bool)parameters[0];
            int? food = Convert.ToInt32(parameters[1]);
            int? gold = Convert.ToInt32(parameters[2]);
            Node<Vector2> currentNode = (Node<Vector2>)parameters[3];
            Action OnWait = parameters[4] as Action;


            behaviours.AddMultiThreadableBehaviours(0, () => { OnWait?.Invoke(); });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (retreat)
                {
                    if (currentNode.NodeType != NodeType.TownCenter)
                    {
                        OnFlag?.Invoke(RTSAgent.Flags.OnRetreat);
                    }
                    return;
                }

                if (currentNode.NodeType == NodeType.Empty || 
                    (currentNode.NodeType == NodeType.Mine && currentNode.gold <= 0))
                {
                    OnFlag?.Invoke(RTSAgent.Flags.OnTargetLost);
                    return;
                }

                if (food > 0 && currentNode.NodeType == NodeType.Mine)
                {
                    OnFlag?.Invoke(RTSAgent.Flags.OnGather);
                    return;
                }

                if (gold <= 0 && currentNode.NodeType == NodeType.TownCenter)
                {
                    OnFlag?.Invoke(RTSAgent.Flags.OnGather);
                    return;
                }
            });

            return behaviours;
        }

        public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            Node<Vector2> currentNode = parameters[0] as Node<Vector2>;
            Action<Node<Vector2>> onReachMine = parameters[1] as Action<Node<Vector2>>;

            behaviours.AddMainThreadBehaviours(0, () =>
            {
                if (currentNode.NodeType == NodeType.Mine) onReachMine?.Invoke(currentNode);
            });

            return behaviours;
        }

        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            Node<Vector2> currentNode = parameters[0] as Node<Vector2>;
            Action<Node<Vector2>> onLeaveMine = parameters[1] as Action<Node<Vector2>>;

            behaviours.AddMainThreadBehaviours(0, () =>
            {
                if (currentNode.NodeType == NodeType.Mine) onLeaveMine?.Invoke(currentNode);
            });

            return behaviours;
        }
    }
}