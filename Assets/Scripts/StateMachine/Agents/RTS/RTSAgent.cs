using System.Collections.Generic;
using Game;
using Pathfinder;
using StateMachine.States.RTSStates;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

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
        }

        public static Node<Vector2> townCenter;

        public float speed = 1.0f;
        public bool retreat;
        public int food;
        
        public Node<System.Numerics.Vector2> currentNode;
        public Node<System.Numerics.Vector2> targetNode;
        private FSM<Behaviours, Flags> _fsm;

        private AStarPathfinder<Node<System.Numerics.Vector2>> _pathfinder;

        private List<Node<System.Numerics.Vector2>> path;
        private int _currentGold;
        private int _lastTimeEat;
        private const int GoldPerFood = 3;
        private const int GoldLimit = 15;

        private void Start()
        {
            Init();
        }

        private void Update()
        {
            _fsm.Tick();
        }

        private void Init()
        {
            _fsm = new FSM<Behaviours, Flags>();
            targetNode = MapGenerator.nodes.Find(x => x.NodeType == NodeType.Mine && x.gold > 0);
            //_pathfinder = new AStarPathfinder<Node<Vector2>>(MapGenerator.nodes, 0, 0);
            //path = _pathfinder.FindPath(currentNode, targetNode);
            _fsm.AddBehaviour<WaitState>(Behaviours.Wait, WaitTickParameters);
            _fsm.AddBehaviour<WalkState>(Behaviours.Walk, WalkTickParameters);
            _fsm.AddBehaviour<GatherGoldState>(Behaviours.GatherResources, GatherTickParameters);


            WalkTransitions();
            GatherTeransitions();
            WaitTransitions();

            
            _fsm.ForceTransition(Behaviours.Walk);
        }

        private void WaitTransitions()
        {
            _fsm.SetTransition(Behaviours.Wait, Flags.OnGather, Behaviours.Walk,
                () =>
                {
                    targetNode = MapGenerator.nodes.Find(x => x.NodeType == NodeType.Mine && x.gold > 0);
                    //path = _pathfinder.FindPath(currentNode, targetNode);
                    Debug.Log("walk to " + targetNode.GetCoordinate());
                });
        }

        private void GatherTeransitions()
        {
            _fsm.SetTransition(Behaviours.GatherResources, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    targetNode = townCenter;
                    path = _pathfinder.FindPath(currentNode, targetNode);
                    Debug.Log("walk to " + targetNode.GetCoordinate());
                });
            _fsm.SetTransition(Behaviours.GatherResources, Flags.OnHunger, Behaviours.Wait, () => Debug.Log("Wait"));
            _fsm.SetTransition(Behaviours.GatherResources, Flags.OnFull, Behaviours.Walk,
                () =>
                {
                    targetNode = townCenter;
                    Debug.Log("Gold full. Walk to " + targetNode.GetCoordinate());
                });
        }


        private object[] GatherTickParameters()
        {
            object[] objects = { retreat, food, _currentGold, _lastTimeEat, GoldPerFood, GoldLimit };
            return objects;
        }


        private void WalkTransitions()
        {
            _fsm.SetTransition(Behaviours.Walk, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    targetNode = townCenter;
                    //path = _pathfinder.FindPath(currentNode, targetNode);
                    Debug.Log("Retreat. Walk to " + targetNode.GetCoordinate());

                });

            _fsm.SetTransition(Behaviours.Walk, Flags.OnTargetLost, Behaviours.Walk,
                () =>
                {
                    targetNode = MapGenerator.nodes.Find(x => x.NodeType == NodeType.Mine && x.gold > 0);
                    //path = _pathfinder.FindPath(currentNode, targetNode);
                    Debug.Log("Retreat. Walk to " + targetNode.GetCoordinate());
                });
            _fsm.SetTransition(Behaviours.Walk, Flags.OnGather, Behaviours.GatherResources,
                () => Debug.Log("Gather gold"));
            _fsm.SetTransition(Behaviours.Walk, Flags.OnWait, Behaviours.Wait, () => Debug.Log("Wait"));
        }

        private object[] WalkTickParameters()
        {
            object[] objects = { currentNode, targetNode, speed, retreat };
            return objects;
        }

        private object[] WalkEnterParameters()
        {
            object[] objects = { currentNode, targetNode, _pathfinder };
            return objects;
        }

        private object[] WaitTickParameters()
        {
            object[] objects = { retreat, food };
            return objects;
        }
    }
}