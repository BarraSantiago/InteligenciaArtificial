using System;
using StateMachine.States.RTSStates;
using UnityEngine;

namespace StateMachine.Agents.RTS
{
    public class Miner : RTSAgent
    {
        private Action onMine;
        public override void Init()
        {
            base.Init();
            _fsm.ForceTransition(Behaviours.Walk);
            onMine += Mine;
        }

        private void Mine()
        {
            if (Food <= 0) return;

            _currentGold++;
                
            _lastTimeEat++;
            currentNode.gold--;

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
                    targetNode = townCenter;
                    _path = _pathfinder.FindPath(currentNode, targetNode);
                    pathNodeId = 0;
                    Debug.Log("Gold full. Walk to " + targetNode.GetCoordinate().x + " - " + targetNode.GetCoordinate().y);
                });
        }
        
        protected override object[] GatherTickParameters()
        {
            return new object[] {false, Food, _currentGold, GoldLimit, onMine};
        }

        protected override void WalkTransitions()
        {
            base.WalkTransitions();
            _fsm.SetTransition(Behaviours.Walk, Flags.OnGather, Behaviours.GatherResources,
                () => Debug.Log("Gather gold"));
        }
    }
}