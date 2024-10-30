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
            var outputBrain1 = (float[])parameters[3];
            var outputBrain2 = (float[])parameters[4];
            
            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                if(currentNode is not { food: > 0 } || foodTarget != currentNode.NodeType) return;
                
                onEat?.Invoke();
            });
            
            behaviours.SetTransitionBehaviour( () =>
            {
                if(currentNode is not { food: > 0 } || foodTarget != currentNode.NodeType) OnFlag?.Invoke(SimAgent.Flags.OnEat);
                
                if(outputBrain1[0] > 0.5f) OnFlag?.Invoke(SimAgent.Flags.OnEat);
                if(outputBrain1[1] > 0.5f) OnFlag?.Invoke(SimAgent.Flags.OnSearchFood);
                if(outputBrain2[0] > 0.5f) OnFlag?.Invoke(SimAgent.Flags.OnEscape);
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