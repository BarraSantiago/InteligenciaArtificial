using System;
using Pathfinder;
using StateMachine.Agents.Simulation;
using States;
using UnityEngine;

namespace StateMachine.States.SimStates
{
    public class SimWalkState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            var behaviours = new BehaviourActions();

            var currentNode = parameters[0] as SimNode<Vector2>;
            var targetNode = parameters[1] as RTSNode<Vector2>;
            var position = (Transform)parameters[2];
            var foodTarget = (SimNodeType)parameters[3];
            var onMove = parameters[4] as Action;
            var outputBrain1 = (float[])parameters[5];
            var outputBrain2 = (float[])parameters[6];

            behaviours.AddMultiThreadableBehaviours(0, () => { onMove?.Invoke(); });

            behaviours.AddMainThreadBehaviours(1, () =>
            {
                if (currentNode == null) return;

                position.position = new Vector3(currentNode.GetCoordinate().x, currentNode.GetCoordinate().y);
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (outputBrain1[0] > 0.5f && currentNode != null && currentNode.NodeType == foodTarget)
                    OnFlag?.Invoke(Flags.OnEat);
                if (outputBrain1[1] > 0.5f) OnFlag?.Invoke(Flags.OnSearchFood);
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

            var currentNode = parameters[0] as SimNode<Vector2>;
            var position = (Transform)parameters[1];
            var foodTarget = (SimNodeType)parameters[2];
            var onMove = parameters[3] as Action;
            var outputBrain1 = (float[])parameters[4];

            behaviours.AddMultiThreadableBehaviours(0, () => { onMove?.Invoke(); });

            behaviours.AddMainThreadBehaviours(1, () =>
            {
                if (currentNode == null) return;

                position.position = new Vector3(currentNode.GetCoordinate().x, currentNode.GetCoordinate().y);
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (outputBrain1[0] > 0.5f && currentNode != null && currentNode.NodeType == foodTarget)
                    OnFlag?.Invoke(Flags.OnEat);
                if (outputBrain1[1] > 0.5f) OnFlag?.Invoke(Flags.OnSearchFood);
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