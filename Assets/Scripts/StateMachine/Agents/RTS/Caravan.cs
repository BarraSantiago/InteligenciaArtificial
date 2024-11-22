using System;
using Game;
using Pathfinder;
using StateMachine.States.RTSStates;
using UnityEngine;

namespace StateMachine.Agents.RTS
{
    public class Caravan : RTSAgent
    {
        private Action onDeliver;
        private Action onGather;

        public override void Init()
        {
            base.Init();
            AgentType = AgentTypes.Caravan;
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
            CurrentRtsNode.food++;
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
                    if (GameManager.MinesWithMiners == null || GameManager.MinesWithMiners.Count <= 0)
                    {
                        Debug.Log("No mines with miners.");
                        return;
                    }

                    RTSNode<Vector2> target = GameManager.MinesWithMiners[0];
                    if (target == null) return;

                    TargetRtsNode =
                        GameManager.Graph.NodesType.Find(node => node.GetCoordinate() == target.GetCoordinate());
                    if (TargetRtsNode == null) return;

                    Debug.Log("Delivering food to " + TargetRtsNode.GetCoordinate().x + " - " +
                              TargetRtsNode.GetCoordinate().y);
                });
            Fsm.SetTransition(Behaviours.GatherResources, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    TargetRtsNode = GetTarget(RTSNodeType.TownCenter);
                    if (TargetRtsNode == null) return;

                    Debug.Log("Retreat. Walk to " + TargetRtsNode.GetCoordinate().x + " - " +
                              TargetRtsNode.GetCoordinate().y);
                });
        }

        protected override void WalkTransitions()
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
                    if (GameManager.MinesWithMiners == null || GameManager.MinesWithMiners.Count <= 0)
                    {
                        Debug.Log("No mines with miners.");
                        return;
                    }

                    RTSNode<Vector2> target = GameManager.MinesWithMiners[0];
                    if (target == null) return;

                    TargetRtsNode =
                        GameManager.Graph.NodesType.Find(node => node.GetCoordinate() == target.GetCoordinate());
                    if (TargetRtsNode == null) return;

                    Debug.Log("Walk to " + TargetRtsNode.GetCoordinate().x + " - " + TargetRtsNode.GetCoordinate().y);
                });

            Fsm.SetTransition(Behaviours.Walk, Flags.OnGather, Behaviours.Deliver,
                () => Debug.Log("Deliver food"));
            Fsm.SetTransition(Behaviours.Walk, Flags.OnWait, Behaviours.GatherResources,
                () => Debug.Log("Deliver food"));
        }

        protected override object[] GetFoodTickParameters()
        {
            return new object[] { Food, FoodLimit, onGather, Retreat };
        }

        protected override object[] GatherTickParameters()
        {
            return new object[] { Retreat, Food, CurrentGold, GoldLimit, onGather };
        }

        protected override void DeliverTransitions()
        {
            Fsm.SetTransition(Behaviours.Deliver, Flags.OnHunger, Behaviours.Walk,
                () =>
                {
                    TargetRtsNode = TownCenter;
                    if (TargetRtsNode == null) return;

                    Debug.Log("To town center");
                });
            Fsm.SetTransition(Behaviours.Deliver, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    TargetRtsNode = TownCenter;
                    if (TargetRtsNode == null) return;

                    Debug.Log("Retreat. Walk to " + TargetRtsNode.GetCoordinate().x + " - " +
                              TargetRtsNode.GetCoordinate().y);
                });
        }

        private object[] DeliverTickParameters()
        {
            return new object[] { Food, onDeliver, Retreat };
        }
    }
}