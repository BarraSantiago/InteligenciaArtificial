using System;
using Game;
using Pathfinder;
using StateMachine.States.RTSStates;
using UnityEngine;

namespace StateMachine.Agents.RTS
{
    public class Miner : RTSAgent
    {
        private Action onMine;
        public static Action OnEmptyMine;

        public override void Init()
        {
            base.Init();
            _fsm.ForceTransition(Behaviours.Walk);
            onMine += Mine;
        }

        private void Mine()
        {
            if (Food <= 0 || currentNode.gold <= 0) return;

            _currentGold++;

            _lastTimeEat++;
            currentNode.gold--;
            if (currentNode.gold <= 0) OnEmptyMine?.Invoke();

            if (_lastTimeEat < GoldPerFood) return;

            Food--;
            _lastTimeEat = 0;

            if (Food > 0 || currentNode.food <= 0) return;

            Food++;
            currentNode.food--;
        }

        protected override void FsmTransitions()
        {
            base.FsmTransitions();
            GatherTransitions();
            WalkTransitions();
        }

        protected override void FsmBehaviours()
        {
            base.FsmBehaviours();
            _fsm.AddBehaviour<GatherGoldState>(Behaviours.GatherResources, GatherTickParameters);
        }

        protected override void GatherTransitions()
        {
            base.GatherTransitions();
            _fsm.SetTransition(Behaviours.GatherResources, Flags.OnHunger, Behaviours.Wait,
                () => Debug.Log("Wait"));

            _fsm.SetTransition(Behaviours.GatherResources, Flags.OnFull, Behaviours.Walk,
                () =>
                {
                    TargetNode = townCenter;

                    Debug.Log("Gold full. Walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
            _fsm.SetTransition(Behaviours.GatherResources, Flags.OnTargetLost, Behaviours.Walk,
                () =>
                {
                    Vector2 position = transform.position;
                    Node<Vector2> target = voronoi.GetMineCloser(GameManager.graph.CoordNodes.Find(nodeVoronoi =>
                        nodeVoronoi.GetCoordinate() == position));
                    TargetNode = MapGenerator<NodeVoronoi, Vector2>.nodes.Find(node => node.GetCoordinate() == target.GetCoordinate());

                    Debug.Log("Mine empty. Walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
        }

        protected override object[] GatherTickParameters()
        {
            return new object[] { false, Food, _currentGold, GoldLimit, onMine, currentNode };
        }

        protected override void WalkTransitions()
        {
            base.WalkTransitions();
            _fsm.SetTransition(Behaviours.Walk, Flags.OnGather, Behaviours.GatherResources,
                () => Debug.Log("Gather gold"));
        }
    }
}