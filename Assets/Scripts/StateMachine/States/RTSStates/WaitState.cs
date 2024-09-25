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
            
            
            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                OnWait?.Invoke();
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if(retreat) return;
                if (food > 0 && currentNode.NodeType == NodeType.Mine) OnFlag?.Invoke(RTSAgent.Flags.OnGather);
                if(currentNode.NodeType == NodeType.Mine && currentNode.gold <= 0) OnFlag?.Invoke(RTSAgent.Flags.OnTargetLost);
                if (gold <= 0 && currentNode.NodeType == NodeType.TownCenter) OnFlag?.Invoke(RTSAgent.Flags.OnGather);
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