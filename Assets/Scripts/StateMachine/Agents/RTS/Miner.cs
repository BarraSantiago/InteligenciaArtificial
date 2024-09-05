using StateMachine.States.RTSStates;
using UnityEngine;

namespace StateMachine.Agents.RTS
{
    public class Miner : RTSAgent
    {
        public override void Init()
        {
            base.Init();
            _fsm.ForceTransition(Behaviours.Walk);
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
                    targetNode = townCenter;
                    Debug.Log("Gold full. Walk to " + targetNode.GetCoordinate().x + " - " + targetNode.GetCoordinate().y);
                });
        }

        protected override void WalkTransitions()
        {
            base.WalkTransitions();
            _fsm.SetTransition(Behaviours.Walk, Flags.OnGather, Behaviours.GatherResources,
                () => Debug.Log("Gather gold"));
        }
    }
}