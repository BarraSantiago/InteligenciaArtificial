using Game;
using Pathfinder;
using StateMachine.States.RTSStates;
using UnityEngine;

namespace StateMachine.Agents.RTS
{
    public class Caravan : RTSAgent
    {
        public override void Init()
        {
            base.Init();
            _fsm.ForceTransition(Behaviours.GatherResources);
        }

        protected override void FsmBehaviours()
        {
            base.FsmBehaviours();
            _fsm.AddBehaviour<GetFoodState>(Behaviours.GatherResources, GetFoodEnterParameters, GetFoodEnterParameters);
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
                () => { targetNode = MapGenerator.nodes.Find(x => x.NodeType == NodeType.Mine && x.gold > 0); });
        }

        protected override void WalkTransitions()
        {
            _fsm.SetTransition(Behaviours.Walk, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    targetNode = townCenter;
                    Debug.Log("Retreat. Walk to " + targetNode.GetCoordinate().x + " - " +
                              targetNode.GetCoordinate().y);
                });

            _fsm.SetTransition(Behaviours.Walk, Flags.OnTargetLost, Behaviours.Walk,
                () =>
                {
                    targetNode = MapGenerator.nodes.Find(x => x.NodeType == NodeType.Mine && x.gold > 0);
                    Debug.Log("Walk to " + targetNode.GetCoordinate().x + " - " + targetNode.GetCoordinate().y);
                });
            _fsm.SetTransition(Behaviours.Walk, Flags.OnGather, Behaviours.Deliver,
                () => Debug.Log("Deliver food"));
            _fsm.SetTransition(Behaviours.Walk, Flags.OnWait, Behaviours.GatherResources,
                () => Debug.Log("Deliver food"));
        }

        protected override void DeliverTransitions()
        {
            _fsm.SetTransition(Behaviours.Deliver, Flags.OnHunger, Behaviours.Walk,
                () =>
                {
                    targetNode = townCenter;
                    _path = _pathfinder.FindPath(currentNode, targetNode);
                    Debug.Log("To town center");
                });
        }

        private object[] DeliverTickParameters()
        {
            return new object[] { Food, currentNode };
        }
    }
}