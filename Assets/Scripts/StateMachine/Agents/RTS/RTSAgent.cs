using System.Collections.Generic;
using Game;
using Pathfinder;
using StateMachine.States.RTSStates;
using UnityEngine;
using Vector2 = Utils.Vec2Int;

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

        public float speed = 1.0f;
        public bool retreat;
        public Node<Vector2> currentNode;
        public Node<Vector2> targetNode;
        
        protected int? Food = (3);
        private int? _currentGold = 0;
        private int? _lastTimeEat = 0;
        protected FSM<Behaviours, Flags> _fsm;
        protected AStarPathfinder<Node<Vector2>> _pathfinder;
        protected List<Node<Vector2>> _path;
        private const int GoldPerFood = 3;
        private const int GoldLimit = 15;
        private const int FoodLimit = 10;

        private void Update()
        {
            _fsm.Tick();
        }

        public virtual void Init()
        {
            _fsm = new FSM<Behaviours, Flags>();

            targetNode = MapGenerator.nodes.Find(x => x.NodeType == NodeType.Mine && x.gold > 0);
            _pathfinder = new AStarPathfinder<Node<Vector2>>(MapGenerator.nodes, 0, 0);
            _path = _pathfinder.FindPath(currentNode, targetNode);

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
            _fsm.AddBehaviour<WalkState>(Behaviours.Walk, WalkTickParameters);
        }

        protected virtual void GatherTransitions()
        {
            _fsm.SetTransition(Behaviours.GatherResources, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    targetNode = townCenter;
                    Debug.Log("Retreat to " + targetNode.GetCoordinate().x + " - " + targetNode.GetCoordinate().y);
                });
        }


        protected virtual object[] GatherTickParameters()
        {
            object[] objects = { retreat, Food, _currentGold, _lastTimeEat, GoldPerFood, GoldLimit, currentNode };
            return objects;
        }


        protected virtual void WalkTransitions()
        {
            _fsm.SetTransition(Behaviours.Walk, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    targetNode = townCenter;
                    Debug.Log("Retreat. Walk to " + targetNode.GetCoordinate().x + " - " + targetNode.GetCoordinate().y);
                });

            _fsm.SetTransition(Behaviours.Walk, Flags.OnTargetLost, Behaviours.Walk,
                () =>
                {
                    targetNode = MapGenerator.nodes.Find(x => x.NodeType == NodeType.Mine && x.gold > 0);
                    Debug.Log("Walk to " + targetNode.GetCoordinate().x + " - " + targetNode.GetCoordinate().y);
                });

            _fsm.SetTransition(Behaviours.Walk, Flags.OnWait, Behaviours.Wait, () => Debug.Log("Wait"));
        }

        protected virtual object[] WalkTickParameters()
        {
            object[] objects = { currentNode, targetNode, speed, retreat, transform, _path, _pathfinder };
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
                    Debug.Log("walk to " + targetNode.GetCoordinate().x + " - " + targetNode.GetCoordinate().y);
                });
        }

        protected virtual object[] WaitTickParameters()
        {
            object[] objects = { retreat, Food, _currentGold, currentNode };
            return objects;
        }

        protected virtual void GetFoodTransitions()
        {
            return;
        }

        protected virtual object[] GetFoodEnterParameters()
        {
            object[] objects = { Food, FoodLimit };
            return objects;
        }
        
        
        protected virtual void DeliverTransitions()
        {
            return;
        }
        
        
    }
}