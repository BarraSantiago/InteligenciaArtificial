using System;
using System.Collections.Generic;
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
            Eat
        }

        public enum Flags
        {
            OnTargetLost,
            OnEscape,
            OnEat,
            OnSearchFood,
        }

        public static Graph<SimNode<Vector2>, NodeVoronoi, Vector2> graph;
        public SimNode<Vector2> CurrentNode;
        public bool CanReproduce() => Food >= FoodLimit;
        public SimAgentTypes SimAgentType{ get; protected set; }
    
        protected int movement = 3;
        protected SimNodeType foodTarget;
        protected int FoodLimit = 5;
        protected int Food = 0;
        protected int PathNodeId;
        protected FSM<Behaviours, Flags> Fsm;
        protected List<SimNode<Vector2>> Path;
        protected Action OnMove;
        protected Action OnEat;

        protected SimNode<Vector2> TargetNode
        {
            get => targetNode;
            set
            {
                targetNode = value;
                PathNodeId = 0;
            }
        }

        private SimNode<Vector2> targetNode;
        public float[][] output;
        public float[][] input;


        public void Tick()
        {
            Fsm.Tick();
        }

        public virtual void Init()
        {
            Fsm = new FSM<Behaviours, Flags>();

            //Pathfinder = GameManager.MinerPathfinder;

            OnMove += Move;
            OnEat += Eat;

            FsmBehaviours();

            FsmTransitions();

            UpdateInputs();
        }

        protected virtual void UpdateInputs()
        {
            FindFoodInputs();
            EscapeInputs();
            AttackInputs();
        }



        private void FindFoodInputs()
        {
            input[0][0] = CurrentNode.GetCoordinate().x;
            input[0][1] = CurrentNode.GetCoordinate().y;
            SimNode<Vector2> target = GetTarget(foodTarget);
            input[0][2] = target.GetCoordinate().x;
            input[0][3] = target.GetCoordinate().y;
        }
        
        protected virtual void EscapeInputs()
        {
        }
        
        protected virtual void AttackInputs()
        {
        }

        public virtual void Uninit()
        {
            OnMove -= Move;
            OnEat -= Eat;
        }


        protected virtual void FsmTransitions()
        {
            WalkTransitions();
            GatherTransitions();
            EatTransitions();
        }


        protected virtual void FsmBehaviours()
        {
            Fsm.AddBehaviour<SimWalkState>(Behaviours.Walk, WalkTickParameters);
            Fsm.AddBehaviour<SimEatState>(Behaviours.Eat, EatTickParameters);
        }

        protected virtual void GatherTransitions()
        {
        }

        protected virtual void WalkTransitions()
        {
        }

        protected virtual object[] WalkTickParameters()
        {
            object[] objects = { CurrentNode, TargetNode, transform, OnMove, output[0], output[1] };
            return objects;
        }

        protected virtual object[] WalkEnterParameters()
        {
            object[] objects = { };
            return objects;
        }

        protected virtual void EatTransitions()
        {
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

            if (Path.Count <= 0) return;
            if (PathNodeId > Path.Count) PathNodeId = 0;

            CurrentNode = Path[PathNodeId];
            PathNodeId++;
        }

        protected virtual SimNode<Vector2> GetTarget(SimNodeType nodeType = SimNodeType.Empty)
        {
            Vector2 position = transform.position;
            SimNode<Vector2> target = null;
            SimNode<Vector2> nearestNode = null;
            float minDistance = float.MaxValue;

           /* switch (nodeType)
            {
                case SimNodeType.Empty:
                    break;
                case SimNodeType.Blocked:

                    break;
                case SimNodeType.Bush:
                    break;
                case SimNodeType.Corpse:
                    break;
                case SimNodeType.Carrion:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(nodeType), nodeType, null);
            }*/

            foreach (var node in graph.NodesType)
            {
                if(node.NodeType != nodeType) continue;
                float distance = Vector2.Distance(position, node.GetCoordinate());
                if (!(distance < minDistance)) continue;
                
                minDistance = distance;
                nearestNode = node;
            }
            
            return nearestNode;

            //return GameManager.Graph.NodesType.Find(node => node.GetCoordinate() == target.GetCoordinate());
        }
    }
}