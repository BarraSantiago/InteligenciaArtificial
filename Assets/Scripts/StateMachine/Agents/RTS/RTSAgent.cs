using System.Collections.Generic;
using Game;
using Pathfinder;
using StateMachine.States.RTSStates;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace StateMachine.Agents.RTS
{
    public class refInt
    {
        public refInt(int value)
        {
            this.value = value;
        }

        public int value;
    }

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

        public float speed = 1.0f;
        public bool retreat;
        public refInt food = new refInt(3);
        public Node<Vector2> currentNode;
        public Node<Vector2> targetNode;

        protected FSM<Behaviours, Flags> _fsm;
        protected AStarPathfinder<Node<Vector2>> _pathfinder;
        protected List<Node<Vector2>> _path;
        private refInt _currentGold = new refInt(0);
        private refInt _lastTimeEat = new refInt(0);
        private const int GoldPerFood = 3;
        private const int GoldLimit = 15;
        private const int FoodLimit = 10;

        private void Start()
        {
            Init();
        }

        private void Update()
        {
            _fsm.Tick();
        }

        protected virtual void Init()
        {
            _fsm = new FSM<Behaviours, Flags>();

            targetNode = MapGenerator.nodes.Find(x => x.NodeType == NodeType.Mine && x.gold > 0);
            _pathfinder = new AStarPathfinder<Node<Vector2>>(MapGenerator.nodes, 0, 0);
            _path = _pathfinder.FindPath(currentNode, targetNode);

            FsmBehaviours();

            FsmTransitions();

            _fsm.ForceTransition(Behaviours.Walk);
        }

        private void FsmTransitions()
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
            _fsm.AddBehaviour<WalkState>(Behaviours.Walk, WalkTickParameters);
        }

        protected virtual void GatherTransitions()
        {
            _fsm.SetTransition(Behaviours.GatherResources, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    targetNode = townCenter;
                    _path = _pathfinder.FindPath(currentNode, targetNode);
                    Debug.Log("Retreat to " + targetNode.GetCoordinate());
                });
        }


        protected virtual object[] GatherTickParameters()
        {
            object[] objects = { retreat, food, _currentGold, _lastTimeEat, GoldPerFood, GoldLimit, currentNode };
            return objects;
        }


        protected virtual void WalkTransitions()
        {
            _fsm.SetTransition(Behaviours.Walk, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    targetNode = townCenter;
                    _path = _pathfinder.FindPath(currentNode, targetNode);
                    Debug.Log("Retreat. Walk to " + targetNode.GetCoordinate());
                });

            _fsm.SetTransition(Behaviours.Walk, Flags.OnTargetLost, Behaviours.Walk,
                () =>
                {
                    targetNode = MapGenerator.nodes.Find(x => x.NodeType == NodeType.Mine && x.gold > 0);
                    _path = _pathfinder.FindPath(currentNode, targetNode);
                    Debug.Log("Walk to " + targetNode.GetCoordinate());
                });

            _fsm.SetTransition(Behaviours.Walk, Flags.OnWait, Behaviours.Wait, () => Debug.Log("Wait"));
        }

        protected virtual object[] WalkTickParameters()
        {
            object[] objects = { currentNode, targetNode, speed, retreat, transform, _path };
            return objects;
        }

        protected virtual object[] WalkEnterParameters()
        {
            object[] objects = { currentNode, targetNode, _pathfinder };
            return objects;
        }

        protected virtual void WaitTransitions()
        {
            _fsm.SetTransition(Behaviours.Wait, Flags.OnGather, Behaviours.Walk,
                () =>
                {
                    targetNode = MapGenerator.nodes.Find(x => x.NodeType == NodeType.Mine && x.gold > 0);
                    _path = _pathfinder.FindPath(currentNode, targetNode);
                    Debug.Log("walk to " + targetNode.GetCoordinate());
                });
        }

        protected virtual object[] WaitTickParameters()
        {
            object[] objects = { retreat, food, _currentGold, currentNode };
            return objects;
        }

        protected virtual void GetFoodTransitions()
        {
            return;
        }

        protected virtual object[] GetFoodEnterParameters()
        {
            object[] objects = { food, FoodLimit };
            return objects;
        }
        
        
        protected virtual void DeliverTransitions()
        {
            return;
        }
        
        
    }
}