using System;
using Pathfinder;
using StateMachine.Agents.Simulation;
using States;
using Utils;

namespace StateMachine.States.SimStates
{
    public class SimEatState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            var behaviours = new BehaviourActions();
            var currentNode = parameters[0] as SimNode<IVector>;
            var foodTarget = (SimNodeType)parameters[1];
            var onEat = parameters[2] as Action;
            var outputBrain1 = (float[])parameters[3];
            var outputBrain2 = (float[])parameters[4];

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                if (currentNode is not { Food: > 0 } || foodTarget != currentNode.NodeType) return;

                onEat?.Invoke();
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (currentNode is not { Food: > 0 } || foodTarget != currentNode.NodeType)
                    OnFlag?.Invoke(Flags.OnSearchFood);

                if (outputBrain1[0] > 0.5f && currentNode != null && currentNode.NodeType == foodTarget)
                    OnFlag?.Invoke(Flags.OnEat);

                SpecialAction(outputBrain2);
            });

            return behaviours;
        }

        protected virtual void SpecialAction(float[] outputBrain2)
        {
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

    public class SimEatScavState : SimEatState
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            var behaviours = new BehaviourActions();
            var currentPos = parameters[0] as IVector;
            var foodNode = parameters[1] as SimNode<IVector>;
            var onEat = parameters[2] as Action;
            var outputBrain1 = (float[])parameters[3];

            IVector distanceToFood = new MyVector();
            IVector maxDistance = new MyVector(4, 4);

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                distanceToFood = new MyVector(foodNode.GetCoordinate().X - currentPos.X,
                    foodNode.GetCoordinate().Y - currentPos.Y);

                if (foodNode is not { Food: > 0 } || foodNode.NodeType != SimNodeType.Carrion ||
                    distanceToFood.Magnitude() > maxDistance.Magnitude()) return;

                onEat?.Invoke();
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (foodNode is not { Food: > 0 } || distanceToFood.Magnitude() > maxDistance.Magnitude())
                    OnFlag?.Invoke(Flags.OnSearchFood);

                if (outputBrain1[0] > 0.5f && currentPos != null &&
                    distanceToFood.Magnitude() <= maxDistance.Magnitude()) OnFlag?.Invoke(Flags.OnEat);
            });

            return behaviours;
        }
    }

    public class SimEatHerbState : SimEatState
    {
        protected override void SpecialAction(float[] outputs)
        {
            if (outputs[0] > 0.5f) OnFlag?.Invoke(Flags.OnEscape);
        }
    }

    public class SimEatCarnState : SimEatState
    {
        protected override void SpecialAction(float[] outputs)
        {
            if (outputs[0] > 0.5f) OnFlag?.Invoke(Flags.OnAttack);
        }
    }
}