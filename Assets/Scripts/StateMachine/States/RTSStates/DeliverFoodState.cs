using System.Numerics;
using Pathfinder;
using StateMachine.Agents.RTS;
using States;
using Utils;

namespace StateMachine.States.RTSStates
{
    public class DeliverFoodState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();
            refInt food = parameters[0] as refInt;
            Node<Vec2Int> node = parameters[1] as Node<Vec2Int>;

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                if (food.value <= 0) return;

                food.value--;
                node.food++;
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (food.value <= 0) OnFlag?.Invoke(RTSAgent.Flags.OnHunger);
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