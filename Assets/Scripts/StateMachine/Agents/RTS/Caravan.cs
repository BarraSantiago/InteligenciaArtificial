using System;
using Game;
using Pathfinder;
using StateMachine.States.RTSStates;
using UnityEngine;

namespace StateMachine.Agents.RTS
{
    public class Caravan : RTSAgent
    {
        private Action onGather;
        private Action onDeliver;

        public override void Init()
        {
            base.Init();
            _fsm.ForceTransition(Behaviours.GatherResources);
            onGather += Gather;
            onDeliver += DeliverFood;
        }

        private void Gather()
        {
            Food++;
        }

        private void DeliverFood()
        {
            if (Food <= 0) return;

            Food--;
            currentNode.food++;
        }

        protected override void FsmBehaviours()
        {
            base.FsmBehaviours();
            _fsm.AddBehaviour<GetFoodState>(Behaviours.GatherResources, GetFoodTickParameters);
            _fsm.AddBehaviour<DeliverFoodState>(Behaviours.Deliver, DeliverTickParameters);
        }

        protected override void FsmTransitions()
        {
            base.FsmTransitions();
            GetFoodTransitions();
            WalkTransitions();
            DeliverTransitions();
        }

        protected override void GetFoodTransitions()
        {
            _fsm.SetTransition(Behaviours.GatherResources, Flags.OnFull, Behaviours.Walk,
                () =>
                {
                    Vector2 position = transform.position;
                    Node<Vector2> target = voronoi.GetMineCloser(GameManager.graph.CoordNodes.Find((nodeVoronoi =>
                        nodeVoronoi.GetCoordinate() == position)));
                    TargetNode = GameManager.graph.NodesType.Find((node => node.GetCoordinate() == target.GetCoordinate()));
                    Debug.Log("Get Food.");
                });
            _fsm.SetTransition(Behaviours.GatherResources, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    TargetNode = townCenter;
                    
                    Debug.Log("Retreat. Walk to " + TargetNode.GetCoordinate().x + " - " +
                              TargetNode.GetCoordinate().y);
                });
        }

        protected override void WalkTransitions()
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
                    Node<Vector2> target = voronoi.GetMineCloser(GameManager.graph.CoordNodes.Find((nodeVoronoi =>
                        nodeVoronoi.GetCoordinate() == position)));
                    TargetNode = GameManager.graph.NodesType.Find((node => node.GetCoordinate() == target.GetCoordinate()));
                    
                    Debug.Log("Walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
            _fsm.SetTransition(Behaviours.Walk, Flags.OnGather, Behaviours.Deliver,
                () => Debug.Log("Deliver food"));
            _fsm.SetTransition(Behaviours.Walk, Flags.OnWait, Behaviours.GatherResources,
                () => Debug.Log("Deliver food"));
        }

        private object[] GetFoodTickParameters()
        {
            return new object[] { Food, FoodLimit, onGather, retreat };
        }

        protected override object[] GatherTickParameters()
        {
            return new object[] { retreat, Food, _currentGold, GoldLimit, onGather };
        }

        protected override void DeliverTransitions()
        {
            _fsm.SetTransition(Behaviours.Deliver, Flags.OnHunger, Behaviours.Walk,
                () =>
                {
                    TargetNode = townCenter;
                    
                    Debug.Log("To town center");
                });
            _fsm.SetTransition(Behaviours.Deliver, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    TargetNode = townCenter;
                    
                    Debug.Log("Retreat. Walk to " + TargetNode.GetCoordinate().x + " - " +
                              TargetNode.GetCoordinate().y);
                });
        }

        private object[] DeliverTickParameters()
        {
            return new object[] { Food, onDeliver, retreat };
        }
    }
}