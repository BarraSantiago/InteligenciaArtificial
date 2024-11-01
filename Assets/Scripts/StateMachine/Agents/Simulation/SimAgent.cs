using System;
using System.Collections.Generic;
using NeuralNetworkDirectory.NeuralNet;
using Pathfinder;
using Pathfinder.Graph;
using StateMachine.States.SimStates;
using UnityEngine;

namespace StateMachine.Agents.Simulation
{
    public class SimAgent : MonoBehaviour
    {
        public enum SimAgentTypes
        {
            Carnivorous,
            Herbivore,
            Scavenger
        }

        public enum Behaviours
        {
            Walk,
            Escape,
            Eat,
            Attack
        }

        public enum Flags
        {
            OnTargetLost,
            OnEscape,
            OnEat,
            OnSearchFood,
            OnAttack
        }

        public static Graph<SimNode<Vector2>, NodeVoronoi, Vector2> graph;
        public NodeVoronoi CurrentNode;
        public bool CanReproduce() => Food >= FoodLimit;
        public SimAgentTypes SimAgentType { get; protected set; }

        protected int movement = 3;
        protected SimNodeType foodTarget;
        protected int FoodLimit = 5;
        protected int Food = 0;
        protected FSM<Behaviours, Flags> Fsm;
        protected Action OnMove;
        protected Action OnEat;

        protected SimNode<Vector2> TargetNode
        {
            get => targetNode;
            set { targetNode = value; }
        }

        private SimNode<Vector2> targetNode;
        public float[][] output;
        public float[][] input;

        public virtual void Init()
        {
            Fsm = new FSM<Behaviours, Flags>();

            OnMove += Move;
            OnEat += Eat;

            FsmBehaviours();

            FsmTransitions();

            UpdateInputs();
        }

        public virtual void Uninit()
        {
            OnMove -= Move;
            OnEat -= Eat;
        }

        public void Tick()
        {
            Fsm.Tick();
            UpdateInputs();
        }

        protected virtual void UpdateInputs()
        {
            FindFoodInputs();
            ExtraInputs();
            MovementInputs();
        }


        private void FindFoodInputs()
        {
            int brain = (int)BrainType.Eat;
            input[brain][0] = CurrentNode.GetCoordinate().x;
            input[brain][1] = CurrentNode.GetCoordinate().y;
            SimNode<Vector2> target = GetTarget(foodTarget);
            input[brain][2] = target.GetCoordinate().x;
            input[brain][3] = target.GetCoordinate().y;
        }

        protected virtual void MovementInputs()
        {
        }

        protected virtual void ExtraInputs()
        {
        }

        protected virtual void FsmTransitions()
        {
            WalkTransitions();
            EatTransitions();
            ExtraTransitions();
        }

        protected virtual void WalkTransitions()
        {
        }

        protected virtual void EatTransitions()
        {
        }

        protected virtual void ExtraTransitions()
        {
        }

        protected virtual void FsmBehaviours()
        {
            Fsm.AddBehaviour<SimWalkState>(Behaviours.Walk, WalkTickParameters);
            ExtraBehaviours();
        }

        protected virtual void ExtraBehaviours()
        {
        }

        protected virtual object[] WalkTickParameters()
        {
            int extraBrain = SimAgentType == SimAgentTypes.Carnivorous ? (int)BrainType.Attack : (int)BrainType.Escape;
            object[] objects =
            {
                CurrentNode, TargetNode, transform, foodTarget, OnMove, output[(int)BrainType.Movement],
                output[extraBrain]
            };
            return objects;
        }

        protected virtual object[] WalkEnterParameters()
        {
            object[] objects = { };
            return objects;
        }


        protected virtual object[] EatTickParameters()
        {
            object[] objects = { CurrentNode, foodTarget, OnEat, output[0], output[1] };
            return objects;
        }

        private void Eat() => Food++;

        protected virtual void Move()
        {
            if (CurrentNode == null || TargetNode == null) return;

            if (CurrentNode.GetCoordinate().Equals(TargetNode.GetCoordinate())) return;

            int brain = (int)BrainType.Movement;

            // TODO - Refactor this
            var targetPos = CurrentNode.GetCoordinate();
            float speed = output[brain][2];
            if (speed < 1) speed = movement;
            if (speed < 0) speed = movement - 1;
            if (speed < -0.6) speed = movement - 2;

            // X axis
            if (output[brain][0] > 0)
            {
                if (output[brain][1] > 0.1) // Right
                {
                    targetPos.x += speed;
                }
                else if (output[brain][1] < -0.1) // left
                {
                    targetPos.x -= speed;
                }
                else
                {
                    // No movement
                }
            }
            else // Y Axis
            {
                if (output[brain][1] > 0.1) // Up
                {
                    targetPos.y += 3;
                }
                else if (output[brain][1] < -0.1) // Down
                {
                    targetPos.y -= 3;
                }
                else
                {
                    // No movement
                }
            }

            if (targetPos != Vector2.zero) CurrentNode = GetNode(targetPos);
        }

        protected virtual SimNode<Vector2> GetTarget(SimNodeType nodeType = SimNodeType.Empty)
        {
            Vector2 position = transform.position;
            SimNode<Vector2> nearestNode = null;
            float minDistance = float.MaxValue;

            foreach (var node in graph.NodesType)
            {
                if (node.NodeType != nodeType) continue;
                float distance = Vector2.Distance(position, node.GetCoordinate());
                if (!(distance < minDistance)) continue;

                minDistance = distance;
                nearestNode = node;
            }

            return nearestNode;
        }

        protected virtual NodeVoronoi GetNode(Vector2 position)
        {
            return graph.CoordNodes[(int)position.x, (int)position.y];
        }
    }
}