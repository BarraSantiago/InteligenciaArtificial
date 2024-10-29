using System;
using System.Collections.Generic;
using Pathfinder;
using Pathfinder.Voronoi;
using StateMachine.States.RTSStates;
using UnityEngine;

namespace StateMachine.Agents.Simulation
{
    public class SimAgent : MonoBehaviour
    {
        public enum AgentTypes
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
            OnTargetReach,
            OnTargetLost,
            OnEscape,
            OnFull,
            OnGather,
        }

        public bool Retreat;
        public SimNode<Vector2> CurrentNode;
        public AStarPathfinder<SimNode<Vector2>, Vector2, NodeVoronoi> Pathfinder;
        public Voronoi<NodeVoronoi, Vector2> Voronoi;

        protected SimNodeType foodTarget;
        protected int FoodLimit = 5;
        protected int Food = 0;
        protected int PathNodeId;
        protected FSM<Behaviours, Flags> Fsm;
        protected List<SimNode<Vector2>> Path;
        protected AgentTypes AgentType;
        protected Action OnMove;
        protected SimNode<Vector2> TargetRtsNode
        {
            get => targetRtsNode;
            set
            {
                targetRtsNode = value;
                Path = Pathfinder.FindPath(CurrentNode, TargetRtsNode);
                PathNodeId = 0;
            }
        }
        
        private SimNode<Vector2> targetRtsNode;

        private void Update()
        {
            Fsm.Tick();
        }

        public virtual void Init()
        {
            Fsm = new FSM<Behaviours, Flags>();

            //Pathfinder = GameManager.MinerPathfinder;

            OnMove += Move;

            FsmBehaviours();

            FsmTransitions();
        }


        protected virtual void FsmTransitions()
        {
            WalkTransitions();
            GatherTransitions();
            GetFoodTransitions();
        }


        protected virtual void FsmBehaviours()
        {
            Fsm.AddBehaviour<WalkState>(Behaviours.Walk, WalkTickParameters, WalkEnterParameters);
        }

        protected virtual void GatherTransitions()
        {
            Fsm.SetTransition(Behaviours.Eat, Flags.OnEscape, Behaviours.Escape,
                () =>
                {
                    TargetRtsNode = GetTarget(SimNodeType.Empty);
                    if (TargetRtsNode == null) return;

                    Debug.Log("Retreat to " + TargetRtsNode.GetCoordinate().x + " - " + TargetRtsNode.GetCoordinate().y);
                });
        }
        
        protected virtual void WalkTransitions()
        {
            Fsm.SetTransition(Behaviours.Walk, Flags.OnEscape, Behaviours.Escape,
                () =>
                {
                    TargetRtsNode = GetTarget(SimNodeType.Empty);
                    if (TargetRtsNode == null) return;

                    Debug.Log("Retreat. Walk to " + TargetRtsNode.GetCoordinate().x + " - " +
                              TargetRtsNode.GetCoordinate().y);
                });

            Fsm.SetTransition(Behaviours.Walk, Flags.OnTargetLost, Behaviours.Walk,
                () =>
                {
                    TargetRtsNode = GetTarget();
                    if (TargetRtsNode == null) return;

                    Debug.Log("Walk to " + TargetRtsNode.GetCoordinate().x + " - " + TargetRtsNode.GetCoordinate().y);
                });
        }

        protected virtual object[] WalkTickParameters()
        {
            object[] objects = { CurrentNode, TargetRtsNode, Retreat, transform, OnMove };
            return objects;
        }

        protected virtual object[] WalkEnterParameters()
        {
            object[] objects = { CurrentNode, TargetRtsNode, Path, Pathfinder, AgentType };
            return objects;
        }

        protected virtual void GetFoodTransitions()
        {
        }

        protected virtual object[] GetFoodTickParameters()
        {
            object[] objects = { Food, FoodLimit };
            return objects;
        }

        protected virtual void Move()
        {
            if (CurrentNode == null || TargetRtsNode == null) return;

            if (CurrentNode.GetCoordinate().Equals(TargetRtsNode.GetCoordinate())) return;

            if (Path.Count <= 0) return;
            if (PathNodeId > Path.Count) PathNodeId = 0;

            CurrentNode = Path[PathNodeId];
            PathNodeId++;
        }

        //TODO Remake get target
        protected virtual SimNode<Vector2> GetTarget(SimNodeType nodeType = SimNodeType.Empty)
        {
            Vector2 position = transform.position;
            SimNode<Vector2> target = null;

            switch (nodeType)
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
            }

            if (target == null)
            {
                return null;
            }

            //return GameManager.Graph.NodesType.Find(node => node.GetCoordinate() == target.GetCoordinate());
            return null;
        }
    }
}