using System;
using Game;
using Pathfinder;
using Pathfinder.Graph;
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
            Fsm.ForceTransition(Behaviours.GatherResources);
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
            CurrentNode.food++;
        }

        protected override void FsmBehaviours()
        {
            base.FsmBehaviours();
            Fsm.AddBehaviour<GetFoodState>(Behaviours.GatherResources, GetFoodTickParameters);
            Fsm.AddBehaviour<DeliverFoodState>(Behaviours.Deliver, DeliverTickParameters);
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
            Fsm.SetTransition(Behaviours.GatherResources, Flags.OnFull, Behaviours.Walk,
                () =>
                {
                    Vector2 position = transform.position;
                    Node<Vector2> target = Voronoi.GetMineCloser(GameManager.Graph.CoordNodes.Find((nodeVoronoi =>
                        nodeVoronoi.GetCoordinate() == position)));
                    TargetNode = Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType.Find(node => node.GetCoordinate() == target.GetCoordinate());
                    Debug.Log("Get Food.");
                });
            Fsm.SetTransition(Behaviours.GatherResources, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    TargetNode = TownCenter;
                    
                    Debug.Log("Retreat. Walk to " + TargetNode.GetCoordinate().x + " - " +
                              TargetNode.GetCoordinate().y);
                });
        }

        protected override void WalkTransitions()
        {
            Fsm.SetTransition(Behaviours.Walk, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    TargetNode = TownCenter;
                    
                    Debug.Log("Retreat. Walk to " + TargetNode.GetCoordinate().x + " - " +
                              TargetNode.GetCoordinate().y);
                });

            Fsm.SetTransition(Behaviours.Walk, Flags.OnTargetLost, Behaviours.Walk,
                () =>
                {
                    Vector2 position = transform.position;
                    Node<Vector2> target = Voronoi.GetMineCloser(GameManager.Graph.CoordNodes.Find((nodeVoronoi =>
                        nodeVoronoi.GetCoordinate() == position)));
                    TargetNode = Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType.Find(node => node.GetCoordinate() == target.GetCoordinate());
                    
                    Debug.Log("Walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
            Fsm.SetTransition(Behaviours.Walk, Flags.OnGather, Behaviours.Deliver,
                () => Debug.Log("Deliver food"));
            Fsm.SetTransition(Behaviours.Walk, Flags.OnWait, Behaviours.GatherResources,
                () => Debug.Log("Deliver food"));
        }

        private object[] GetFoodTickParameters()
        {
            return new object[] { Food, FoodLimit, onGather, retreat };
        }

        protected override object[] GatherTickParameters()
        {
            return new object[] { retreat, Food, CurrentGold, GoldLimit, onGather };
        }

        protected override void DeliverTransitions()
        {
            Fsm.SetTransition(Behaviours.Deliver, Flags.OnHunger, Behaviours.Walk,
                () =>
                {
                    TargetNode = TownCenter;
                    
                    Debug.Log("To town center");
                });
            Fsm.SetTransition(Behaviours.Deliver, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    TargetNode = TownCenter;
                    
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