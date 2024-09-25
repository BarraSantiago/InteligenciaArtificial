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

        public enum Behaviours
        {
            Wait,
            Walk,
            GatherResources,
            Deliver
        }

        public static Node<Vector2> townCenter;

        public bool retreat;
        public Node<Vector2> currentNode;

        private Node<Vector2> targetNode;

        protected Node<Vector2> TargetNode
        {
            get => targetNode;
            set
            {
                targetNode = value;
                _path = _pathfinder.FindPath(currentNode, TargetNode);
                pathNodeId = 0;
            }
        }

        public Voronoi<NodeVoronoi, Vector2> voronoi;

        //private Func<int> gold;

        protected Action OnMove;
        protected Action OnWait;
        protected int? Food = (3);
        protected int? _currentGold = 0;
        protected int? _lastTimeEat = 0;
        protected FSM<Behaviours, Flags> _fsm;
        protected AStarPathfinder<Node<Vector2>, Vector2, NodeVoronoi> _pathfinder;
        protected List<Node<Vector2>> _path;
        protected const int GoldPerFood = 3;
        protected const int GoldLimit = 15;
        protected const int FoodLimit = 10;
        protected int pathNodeId;

        private void Update()
        {
            _fsm.Tick();
        }

        public virtual void Init()
        {
            _fsm = new FSM<Behaviours, Flags>();

            _pathfinder = new AStarPathfinder<Node<Vector2>, Vector2, NodeVoronoi>(MapGenerator<NodeVoronoi, Vector2>.nodes, 0, 0);
            
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
            _fsm.AddBehaviour<WaitState>(Behaviours.Wait, WaitTickParameters);
            _fsm.AddBehaviour<WalkState>(Behaviours.Walk, WalkTickParameters, WalkEnterParameters);
        }

        protected virtual void GatherTransitions()
        {
            _fsm.SetTransition(Behaviours.GatherResources, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    TargetNode = townCenter;
                    
                    Debug.Log("Retreat to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
        }


        protected virtual object[] GatherTickParameters()
        {
            object[] objects = { retreat, Food, _currentGold, GoldLimit };
            return objects;
        }


        protected virtual void WalkTransitions()
        {
            _fsm.SetTransition(Behaviours.Walk, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    TargetNode = townCenter;
                   
                    Debug.Log("Retreat. Walk to " + TargetNode.GetCoordinate().x + " - " +
                              TargetNode.GetCoordinate().y);
                });

            _fsm.SetTransition(Behaviours.Walk, Flags.OnTargetLost, Behaviours.Walk,
                () =>
                {
                    Vector2 position = transform.position;
                    Node<Vector2> target = voronoi.GetMineCloser(GameManager.graph.CoordNodes.Find(nodeVoronoi =>
                        nodeVoronoi.GetCoordinate() == position));
                    TargetNode = MapGenerator<NodeVoronoi, Vector2>.nodes.Find(node => node.GetCoordinate() == target.GetCoordinate());
                   
                    Debug.Log("Walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });

            _fsm.SetTransition(Behaviours.Walk, Flags.OnWait, Behaviours.Wait, () => Debug.Log("Wait"));
        }

        protected virtual object[] WalkTickParameters()
        {
            object[] objects = { currentNode, TargetNode, retreat, transform, OnMove };
            return objects;
        }

        protected virtual object[] WalkEnterParameters()
        {
            object[] objects = { currentNode, TargetNode, _path, _pathfinder };
            return objects;
        }

        protected virtual void WaitTransitions()
        {
            _fsm.SetTransition(Behaviours.Wait, Flags.OnGather, Behaviours.Walk,
                () =>
                {
                    Vector2 position = transform.position;
                    Node<Vector2> target = voronoi.GetMineCloser(GameManager.graph.CoordNodes.Find(nodeVoronoi =>
                        nodeVoronoi.GetCoordinate() == position));
                    TargetNode = MapGenerator<NodeVoronoi, Vector2>.nodes.Find(node => node.GetCoordinate() == target.GetCoordinate());

                    Debug.Log("walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
            _fsm.SetTransition(Behaviours.Wait, Units.Flags.OnTargetLost, Behaviours.Walk,
                () =>
                {
                    Vector2 position = transform.position;
                    Node<Vector2> target = voronoi.GetMineCloser(GameManager.graph.CoordNodes.Find(nodeVoronoi =>
                        nodeVoronoi.GetCoordinate() == position));
                    TargetNode = MapGenerator<NodeVoronoi, Vector2>.nodes.Find(node => node.GetCoordinate() == target.GetCoordinate());

                    Debug.Log("walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
        }

        protected virtual object[] WaitTickParameters()
        {
            object[] objects = { retreat, Food, _currentGold, currentNode, OnWait};
            return objects;
        }

        protected virtual void GetFoodTransitions()
        {
            return;
        }

        protected virtual object[] GetFoodTickParameters()
        {
            object[] objects = { Food, FoodLimit };
            return objects;
        }

        protected virtual void DeliverTransitions()
        {
            return;
        }
        
        private void Move()
        {
            if (currentNode == null || TargetNode == null)
            {
                return;
            }

            if (currentNode.Equals(TargetNode)) return;

            if (_path.Count <= 0) return;
            if(pathNodeId > _path.Count) pathNodeId = 0;

            currentNode = _path[pathNodeId];
            pathNodeId++;
        }

        private void Wait()
        {
            if (currentNode.NodeType == NodeType.Mine && currentNode.food > 0)
            {
                if (Food > 1) return;
                Food++;
                currentNode.food--;
            }

            if (currentNode.NodeType != NodeType.TownCenter || _currentGold < 1) return;

            currentNode.gold++;
            _currentGold--;
        }
    }
}