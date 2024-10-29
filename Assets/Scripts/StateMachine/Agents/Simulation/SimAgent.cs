using System;
using System.Collections.Generic;
using Pathfinder;
using StateMachine.States.SimStates;
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

        public SimNode<Vector2> CurrentNode;
        public bool CanReproduce() => Food >= FoodLimit;

        protected int movement = 3;
        protected SimNodeType foodTarget;
        protected int FoodLimit = 5;
        protected int Food = 0;
        protected int PathNodeId;
        protected FSM<Behaviours, Flags> Fsm;
        protected List<SimNode<Vector2>> Path;
        protected AgentTypes AgentType;
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

        private void Update()
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
            object[] objects = { CurrentNode, TargetNode, transform, OnMove };
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
            object[] objects = { CurrentNode, foodTarget, OnEat };
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