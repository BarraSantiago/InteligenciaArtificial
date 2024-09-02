using System.Numerics;
using Pathfinder;
using StateMachine.Agents.RTS;
using States;

namespace StateMachine.States.RTSStates
{
    public class WaitState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            bool retreat = (bool)parameters[0];
            int food = (int)parameters[1];
            int gold = (int)parameters[2];
            Node<Vector2> currentNode = (Node<Vector2>)parameters[3];

            
            behaviours.AddMainThreadBehaviours(0, () =>
            {
                if (currentNode.NodeType == NodeType.Mine && currentNode.food > 0)
                {
                    if(food > 1) return;
                    food++;
                    currentNode.food--;
                }

                if (currentNode.NodeType != NodeType.TownCenter || gold < 15) return;
                
                currentNode.gold += gold;
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
            throw new System.NotImplementedException();
        }

        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            throw new System.NotImplementedException();
        }
    }
}