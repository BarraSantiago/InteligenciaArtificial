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
            BehaviourActions behaviours = new BehaviourActions();

            SimNode<IVector> currentNode = parameters[0] as SimNode<IVector>;
            SimNodeType foodTarget = (SimNodeType)parameters[1];
            Action onMove = parameters[2] as Action;
            float[] outputBrain1 = (float[])parameters[3];
            float[] outputBrain2 = (float[])parameters[4];

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
            BehaviourActions behaviours = new BehaviourActions();

            IVector position = parameters[0] as IVector;
            IVector nearestFood = parameters[1] as IVector;
            Action onMove = parameters[2] as Action;
            float[] outputBrain1 = (float[])parameters[3];
            IVector distanceToFood = new MyVector();
            IVector maxDistance = new MyVector(4, 4);

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                onMove?.Invoke();
                if (nearestFood == null || position == null) return;
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

    public class SimWalkHerbState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            SimNode<IVector> currentNode = parameters[0] as SimNode<IVector>;
            SimNodeType foodTarget = (SimNodeType)parameters[1];
            Action onMove = parameters[2] as Action;
            float[] outputBrain1 = (float[])parameters[3];
            float[] outputBrain2 = (float[])parameters[4];

            behaviours.AddMultiThreadableBehaviours(0, () => { onMove?.Invoke(); });

            //behaviours.AddMainThreadBehaviours(1, () =>
            //{
            //    if (currentNode == null) return;

            //    position.position = new Vector3(currentNode.GetCoordinate().x, currentNode.GetCoordinate().y);
            //});

            behaviours.SetTransitionBehaviour(() =>
            {
                if (outputBrain1[0] > 0.5f && currentNode != null && currentNode.NodeType == foodTarget)
                    OnFlag?.Invoke(Flags.OnEat);
                if (outputBrain2[0] > 0.5f) OnFlag?.Invoke(Flags.OnEscape);
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