 using System;
 using Pathfinder;
 using StateMachine.Agents.Simulation;
 using States;
 using UnityEngine;

 namespace StateMachine.States.SimStates
{
    public class SimEatState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            var behaviours = new BehaviourActions();
            var currentNode = parameters[0] as SimNode<Vector2>;
            var foodTarget = (SimNodeType)parameters[1];
            var onEat = parameters[2] as Action;

            
            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                if(currentNode is not { food: > 0 } || foodTarget != currentNode.NodeType) return;
                
                onEat?.Invoke();
            });
            
            behaviours.SetTransitionBehaviour( () =>
            {
                if(currentNode is not { food: > 0 } || foodTarget != currentNode.NodeType) OnFlag?.Invoke(SimAgent.Flags.OnGather);
            });
            
            return behaviours;
        }

        public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
        {
            return default;
        }

        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            return default;
        }
    }
}