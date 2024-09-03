using Game;
using Pathfinder;
using StateMachine.States.RTSStates;
using UnityEngine;

namespace StateMachine.Agents.RTS
{
    public class Caravan : RTSAgent
    {
        protected override void Init()
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
        
        protected override void GetFoodTransitions()
        {
            _fsm.SetTransition(Behaviours.GatherResources, Flags.OnFull, Behaviours.Walk,
                () =>
                {
                    targetNode = MapGenerator.nodes.Find(x => x.NodeType == NodeType.Mine && x.gold > 0);
                    _path = _pathfinder.FindPath(currentNode, targetNode);
                });
        }

        protected override void WalkTransitions()
        {
            base.WalkTransitions();
            _fsm.SetTransition(Behaviours.Walk, Flags.OnGather, Behaviours.Deliver,
                () => Debug.Log("Gather food"));
        }

        protected override void DeliverTransitions()
        {
            _fsm.SetTransition(Behaviours.Deliver, Flags.OnHunger, Behaviours.Walk,
                () =>
                {
                    targetNode = townCenter;
                    _path = _pathfinder.FindPath(currentNode, targetNode);
                });
        }

        private object[] DeliverTickParameters()
        {
            return new object[] { food, currentNode };
        }
    }
}