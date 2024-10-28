using System;
using System.Collections.Generic;
using Game;
using Pathfinder;
using Pathfinder.Voronoi;
using StateMachine.States.RTSStates;
using UnityEngine;

namespace StateMachine.Agents.RTS
{
    public class RTSAgent : MonoBehaviour
    {
        public enum AgentTypes
        {
            Miner,
            Caravan
        }

        public enum Behaviours
        {
            Wait,
            Walk,
            GatherResources,
            Deliver
        }

        public enum Flags
        {
            OnTargetReach,
            OnTargetLost,
            OnHunger,
            OnRetreat,
            OnFull,
            OnGather,
            OnWait
        }

        protected const int GoldPerFood = 3;
        protected const int GoldLimit = 15;
        protected const int FoodLimit = 10;

        public static RTSNode<Vector2> TownCenter;

        public static bool Retreat;
        protected AgentTypes AgentType;
        protected int CurrentGold;
        public RTSNode<Vector2> CurrentRtsNode;

        protected int Food = 3;

        protected FSM<Behaviours, Flags> Fsm;
        protected int LastTimeEat = 0;

        protected Action OnMove;
        protected Action OnWait;
        protected List<RTSNode<Vector2>> Path;
        public AStarPathfinder<RTSNode<Vector2>, Vector2, NodeVoronoi> Pathfinder;
        protected int PathNodeId;

        private RTSNode<Vector2> targetRtsNode;
        public Voronoi<NodeVoronoi, Vector2> Voronoi;

        protected RTSNode<Vector2> TargetRtsNode
        {
            get => targetRtsNode;
            set
            {
                targetRtsNode = value;
                Path = Pathfinder.FindPath(CurrentRtsNode, TargetRtsNode);
                PathNodeId = 0;
            }
        }

        private void Update()
        {
            Fsm.Tick();
        }

        public virtual void Init()
        {
            Fsm = new FSM<Behaviours, Flags>();

            Pathfinder = GameManager.MinerPathfinder;

            OnMove += Move;
            OnWait += Wait;

            FsmBehaviours();

            FsmTransitions();
        }


        protected virtual void FsmTransitions()
        {
            WalkTransitions();
            WaitTransitions();
            GatherTransitions();
            GetFoodTransitions();
            DeliverTransitions();
        }


        protected virtual void FsmBehaviours()
        {
            Fsm.AddBehaviour<WaitState>(Behaviours.Wait, WaitTickParameters, WaitEnterParameters, WaitExitParameters);
            Fsm.AddBehaviour<WalkState>(Behaviours.Walk, WalkTickParameters, WalkEnterParameters);
        }

        protected virtual void GatherTransitions()
        {
            Fsm.SetTransition(Behaviours.GatherResources, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    TargetRtsNode = GetTarget(RTSNodeType.TownCenter);
                    if (TargetRtsNode == null) return;

                    Debug.Log("Retreat to " + TargetRtsNode.GetCoordinate().x + " - " + TargetRtsNode.GetCoordinate().y);
                });
        }


        protected virtual object[] GatherTickParameters()
        {
            object[] objects = { Retreat, Food, CurrentGold, GoldLimit };
            return objects;
        }


        protected virtual void WalkTransitions()
        {
            Fsm.SetTransition(Behaviours.Walk, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    TargetRtsNode = GetTarget(RTSNodeType.TownCenter);
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

            Fsm.SetTransition(Behaviours.Walk, Flags.OnWait, Behaviours.Wait, () => Debug.Log("Wait"));
        }

        protected virtual object[] WalkTickParameters()
        {
            object[] objects = { CurrentRtsNode, TargetRtsNode, Retreat, transform, OnMove };
            return objects;
        }

        protected virtual object[] WalkEnterParameters()
        {
            object[] objects = { CurrentRtsNode, TargetRtsNode, Path, Pathfinder, AgentType };
            return objects;
        }

        protected virtual void WaitTransitions()
        {
            Fsm.SetTransition(Behaviours.Wait, Flags.OnGather, Behaviours.Walk,
                () =>
                {
                    TargetRtsNode = GetTarget();
                    if (TargetRtsNode == null) return;

                    Debug.Log("walk to " + TargetRtsNode.GetCoordinate().x + " - " + TargetRtsNode.GetCoordinate().y);
                });
            Fsm.SetTransition(Behaviours.Wait, Units.Flags.OnTargetLost, Behaviours.Walk,
                () =>
                {
                    TargetRtsNode = GetTarget();
                    if (TargetRtsNode == null) return;

                    Debug.Log("walk to " + TargetRtsNode.GetCoordinate().x + " - " + TargetRtsNode.GetCoordinate().y);
                });
            Fsm.SetTransition(Behaviours.Wait, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    TargetRtsNode = GetTarget(RTSNodeType.TownCenter);
                    if (TargetRtsNode == null) return;

                    Debug.Log("Retreat. Walk to " + TargetRtsNode.GetCoordinate().x + " - " +
                              TargetRtsNode.GetCoordinate().y);
                });
        }


        protected virtual object[] WaitTickParameters()
        {
            object[] objects = { Retreat, Food, CurrentGold, CurrentRtsNode, OnWait };
            return objects;
        }

        protected virtual object[] WaitEnterParameters()
        {
            return null;
        }

        protected virtual object[] WaitExitParameters()
        {
            return null;
        }


        protected virtual void GetFoodTransitions()
        {
        }

        protected virtual object[] GetFoodTickParameters()
        {
            object[] objects = { Food, FoodLimit };
            return objects;
        }

        protected virtual void DeliverTransitions()
        {
        }

        protected virtual void Move()
        {
            if (CurrentRtsNode == null || TargetRtsNode == null) return;

            if (CurrentRtsNode.GetCoordinate().Equals(TargetRtsNode.GetCoordinate())) return;

            if (Path.Count <= 0) return;
            if (PathNodeId > Path.Count) PathNodeId = 0;

            CurrentRtsNode = Path[PathNodeId];
            PathNodeId++;
        }

        private void Wait()
        {
            if (CurrentRtsNode.RtsNodeType == RTSNodeType.Mine && CurrentRtsNode.food > 0)
            {
                if (Food > 1) return;
                Food++;
                CurrentRtsNode.food--;
            }

            if (CurrentRtsNode.RtsNodeType != RTSNodeType.TownCenter || CurrentGold < 1) return;

            CurrentRtsNode.gold++;
            CurrentGold--;
        }

        protected RTSNode<Vector2> GetTarget(RTSNodeType rtsNodeType = RTSNodeType.Mine)
        {
            Vector2 position = transform.position;
            RTSNode<Vector2> target;

            switch (rtsNodeType)
            {
                case RTSNodeType.Mine:
                    target = Voronoi.GetMineCloser(GameManager.Graph.CoordNodes.Find(nodeVoronoi =>
                        nodeVoronoi.GetCoordinate() == position));
                    break;

                case RTSNodeType.TownCenter:
                    target = TownCenter;
                    break;

                default:
                    target = Voronoi.GetMineCloser(GameManager.Graph.CoordNodes.Find(nodeVoronoi =>
                        nodeVoronoi.GetCoordinate() == position));
                    break;
            }

            if (target == null)
            {
                Debug.LogError("No mines with gold.");
                return null;
            }

            return GameManager.Graph.NodesType.Find(node => node.GetCoordinate() == target.GetCoordinate());
        }
    }
}