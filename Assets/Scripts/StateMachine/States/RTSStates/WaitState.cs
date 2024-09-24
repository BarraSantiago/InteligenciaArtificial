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


            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                if (currentNode.NodeType == NodeType.Mine && currentNode.food > 0)
                {
                    if (food > 1) return;
                    food++;
                    currentNode.food--;
                }

                if (currentNode.NodeType != NodeType.TownCenter || gold < 15) return;

                currentNode.gold += (int)gold;
                gold = 0;
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (food > 0 && !retreat) OnFlag?.Invoke(RTSAgent.Flags.OnGather);
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