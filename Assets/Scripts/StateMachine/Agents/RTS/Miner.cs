using System;
using Game;
using Pathfinder;
using Pathfinder.Graph;
using StateMachine.States.RTSStates;
using UnityEngine;

namespace StateMachine.Agents.RTS
{
    public class Miner : RTSAgent
    {
        private Action onMine;
        public static Action OnEmptyMine;
        public static Action<Node<Vector2>> OnReachMine;
        public static Action<Node<Vector2>> OnLeaveMine;

        public override void Init()
        {
            base.Init();
            Fsm.ForceTransition(Behaviours.Walk);
            onMine += Mine;
        }

        private void Mine()
        {
            if (Food <= 0 || CurrentNode.gold <= 0) return;

            CurrentGold++;

            LastTimeEat++;
            CurrentNode.gold--;
            if (CurrentNode.gold <= 0) OnEmptyMine?.Invoke();

            if (LastTimeEat < GoldPerFood) return;

            Food--;
            LastTimeEat = 0;

            if (Food > 0 || CurrentNode.food <= 0) return;

            Food++;
            CurrentNode.food--;
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
            Fsm.AddBehaviour<GatherGoldState>(Behaviours.GatherResources, GatherTickParameters, GatherEnterParameters, GatherLeaveParameters);
        }

        protected override void GatherTransitions()
        {
            base.GatherTransitions();
            Fsm.SetTransition(Behaviours.GatherResources, Flags.OnHunger, Behaviours.Wait,
                () => Debug.Log("Wait"));

            Fsm.SetTransition(Behaviours.GatherResources, Flags.OnFull, Behaviours.Walk,
                () =>
                {
                    TargetNode = TownCenter;

                    Debug.Log("Gold full. Walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
            Fsm.SetTransition(Behaviours.GatherResources, Flags.OnTargetLost, Behaviours.Walk,
                () =>
                {
                    Vector2 position = transform.position;
                    Node<Vector2> target = Voronoi.GetMineCloser(GameManager.Graph.CoordNodes.Find(nodeVoronoi =>
                        nodeVoronoi.GetCoordinate() == position));
                    TargetNode = Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType.Find(node => node.GetCoordinate() == target.GetCoordinate());

                    Debug.Log("Mine empty. Walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
        }

        protected override object[] GatherTickParameters()
        {
            return new object[] { Retreat, Food, CurrentGold, GoldLimit, onMine, CurrentNode };
        }
        
        protected object[] GatherEnterParameters()
        {
            return new object[] { OnReachMine, CurrentNode };
        }
        
        protected object[] GatherLeaveParameters()
        {
            return new object[] {OnLeaveMine, CurrentNode };
        }

        protected override void WalkTransitions()
        {
            base.WalkTransitions();
            Fsm.SetTransition(Behaviours.Walk, Flags.OnGather, Behaviours.GatherResources,
                () => Debug.Log("Gather gold"));
        }
    }
}