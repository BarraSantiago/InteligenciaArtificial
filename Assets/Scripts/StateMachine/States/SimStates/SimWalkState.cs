using System;
using Pathfinder;
using StateMachine.Agents.Simulation;
using States;
using UnityEngine;
using Utils;

namespace StateMachine.States.SimStates
{
    public class SimWalkState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            var behaviours = new BehaviourActions();

            var currentNode = parameters[0] as SimNode<Vector2>;
            var foodTarget = (SimNodeType)parameters[1];
            var onMove = parameters[2] as Action;
            var outputBrain1 = (float[])parameters[3];
            var outputBrain2 = (float[])parameters[4];

            behaviours.AddMultiThreadableBehaviours(0, () => { onMove.Invoke(); });

            //behaviours.AddMainThreadBehaviours(1, () =>
            //{
            //    if (currentNode == null) return;

            //    position.position = new Vector3(currentNode.GetCoordinate().x, currentNode.GetCoordinate().y);
            //});

            behaviours.SetTransitionBehaviour(() =>
            {
                if (outputBrain1[0] > 0.5f && currentNode != null && currentNode.NodeType == foodTarget)
                    OnFlag?.Invoke(Flags.OnEat);
                SpecialAction(outputBrain2);
            });
            return behaviours;
        }

        protected virtual void SpecialAction(float[] outputs)
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

    public class SimWalkScavState : SimWalkState
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            var behaviours = new BehaviourActions();

            var position = parameters[0] as IVector;
            var nearestFood = parameters[1] as IVector;
            var onMove = parameters[2] as Action;
            var outputBrain1 = (float[])parameters[3];
            IVector distanceToFood = new MyVector();
            IVector maxDistance = new MyVector(4,4);

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                onMove.Invoke();
                distanceToFood = new MyVector(nearestFood.X - position.X, nearestFood.Y - position.Y);
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (outputBrain1[0] > 0.5f && distanceToFood.Magnitude() < maxDistance.Magnitude())
                    OnFlag?.Invoke(Flags.OnEat);
            });
            return behaviours;
        }
    }

    public class SimWalkHerbState : SimWalkState
    {
        protected override void SpecialAction(float[] outputs)
        {
            if (outputs[0] > 0.5f) OnFlag?.Invoke(Flags.OnEscape);
        }
    }

    public class SimWalkCarnState : SimWalkState
    {
        protected override void SpecialAction(float[] outputs)
        {
            if (outputs[0] > 0.5f) OnFlag?.Invoke(Flags.OnAttack);
        }
    }
}