using System;
using Pathfinder;
using StateMachine.States.RTSStates;
using UnityEngine;

namespace StateMachine.Agents.RTS
{
    public class Miner : RTSAgent
    {
        public static Action OnEmptyMine;
        public static Action<RTSNode<Vector2>> OnReachMine;
        public static Action<RTSNode<Vector2>> OnLeaveMine;
        private Action onMine;

        public override void Init()
        {
            base.Init();
            AgentType = AgentTypes.Miner;
            Fsm.ForceTransition(Behaviours.Walk);
            onMine += Mine;
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
            Fsm.AddBehaviour<GatherGoldState>(Behaviours.GatherResources, GatherTickParameters, GatherEnterParameters,
                GatherLeaveParameters);
        }

        protected override void GatherTransitions()
        {
            base.GatherTransitions();
            Fsm.SetTransition(Behaviours.GatherResources, Flags.OnHunger, Behaviours.Wait,
                () => Debug.Log("Wait"));

            Fsm.SetTransition(Behaviours.GatherResources, Flags.OnFull, Behaviours.Walk,
                () =>
                {
                    TargetRtsNode = GetTarget(RTSNodeType.TownCenter);

                    if (TargetRtsNode == null) return;

                    Debug.Log("Gold full. Walk to " + TargetRtsNode.GetCoordinate().x + " - " +
                              TargetRtsNode.GetCoordinate().y);
                });
            Fsm.SetTransition(Behaviours.GatherResources, Flags.OnTargetLost, Behaviours.Walk,
                () =>
                {
                    TargetRtsNode = GetTarget();
                    if (TargetRtsNode == null) return;
                    Debug.Log("Mine empty. Walk to " + TargetRtsNode.GetCoordinate().x + " - " +
                              TargetRtsNode.GetCoordinate().y);
                });
        }

        protected override object[] GatherTickParameters()
        {
            return new object[] { Retreat, Food, CurrentGold, GoldLimit, onMine, CurrentRtsNode };
        }

        protected object[] GatherEnterParameters()
        {
            return new object[] { OnReachMine, CurrentRtsNode };
        }

        protected object[] GatherLeaveParameters()
        {
            return new object[] { OnLeaveMine, CurrentRtsNode };
        }

        protected override void WalkTransitions()
        {
            base.WalkTransitions();
            Fsm.SetTransition(Behaviours.Walk, Flags.OnGather, Behaviours.GatherResources,
                () => Debug.Log("Gather gold"));
        }

        protected override object[] WaitEnterParameters()
        {
            return new object[] { CurrentRtsNode, OnReachMine };
        }

        protected override object[] WaitExitParameters()
        {
            return new object[] { CurrentRtsNode, OnLeaveMine };
        }

        private void Mine()
        {
            if (Food <= 0 || CurrentRtsNode.gold <= 0) return;

            CurrentGold++;

            LastTimeEat++;
            CurrentRtsNode.gold--;
            if (CurrentRtsNode.gold <= 0) OnEmptyMine?.Invoke();

            if (LastTimeEat < GoldPerFood) return;

            Food--;
            LastTimeEat = 0;

            if (Food > 0 || CurrentRtsNode.food <= 0) return;

            Food++;
            CurrentRtsNode.food--;
        }
    }
}