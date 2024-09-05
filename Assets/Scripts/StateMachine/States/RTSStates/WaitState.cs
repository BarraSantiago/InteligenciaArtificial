using System.Numerics;
using Pathfinder;
using StateMachine.Agents.RTS;
using States;
using Utils;

namespace StateMachine.States.RTSStates
{
    public class WaitState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            bool retreat = (bool)parameters[0];
            refInt food = parameters[1] as refInt;
            refInt gold = parameters[2] as refInt;
            Node<Vec2Int> currentNode = (Node<Vec2Int>)parameters[3];

            
            behaviours.AddMainThreadBehaviours(0, () =>
            {
                if (currentNode.NodeType == NodeType.Mine && currentNode.food > 0)
                {
                    if(food.value > 1) return;
                    food.value++;
                    currentNode.food--;
                }

                if (currentNode.NodeType != NodeType.TownCenter || gold.value < 15) return;
                
                currentNode.gold += gold.value;
                gold.value = 0;
            });
            
            behaviours.SetTransitionBehaviour(() =>
            {
                if (food.value > 0 && !retreat) OnFlag?.Invoke(RTSAgent.Flags.OnGather);
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