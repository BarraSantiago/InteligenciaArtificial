using System;
using System.Collections.Generic;

namespace DefaultNamespace
{
    public class FSM
    {
        private const int UNNASIGNED_TRANSITION = -1;
        private int stateAmount = 0;
        private int currentState = 0;
        private Dictionary<int, State> behaviours;
        private Dictionary<int, Func<object[]>> _behaviourTickParameters;
        private Dictionary<int, Func<object[]>> _behaviourOnEnterParameters;
        private Dictionary<int, Func<object[]>> _behaviourOnExitParameters;
        private int[,] transitions;


        public FSM(int statesAmount, int flags)
        {
            behaviours = new Dictionary<int, State>();
            transitions = new int[statesAmount, flags];

            for (int i = 0; i < statesAmount; i++)
            {
                for (int j = 0; j < flags; j++)
                {
                    transitions[i, j] = UNNASIGNED_TRANSITION;
                }
            }

            _behaviourTickParameters = new Dictionary<int, Func<object[]>>();
            _behaviourOnEnterParameters = new Dictionary<int, Func<object[]>>();
            _behaviourOnExitParameters = new Dictionary<int, Func<object[]>>();
        }

        public void Force(int state)
        {
            currentState = state;
        }
        
        public void AddBehaviour<T>(int stateIndex, Func<object[]> onTickParameters = null,
            Func<object[]> onEnterParameters = null, Func<object[]> onExitParameters = null) where T : State, new()
        {
            if (behaviours.ContainsKey(stateIndex)) return;

            State newBehaviour = new T();
            newBehaviour.OnFlag += Transition;
            behaviours.Add(stateIndex, newBehaviour);
            _behaviourTickParameters.Add(stateIndex, onTickParameters);
            _behaviourOnEnterParameters.Add(stateIndex, onEnterParameters);
            _behaviourOnExitParameters.Add(stateIndex, onExitParameters);
        }

        public void SetTransition(int originState, int flag, int destinationState)
        {
            transitions[originState, flag] = destinationState;
        }

        public void Transition(int flag)
        {
            foreach (Action behaviour in behaviours[currentState]
                         .GetOnExitBehaviour(_behaviourOnEnterParameters[currentState]?.Invoke()))
            {
                behaviour.Invoke();
            }

            currentState = transitions[currentState, flag];
            foreach (Action behaviour in behaviours[currentState]
                         .GetOnEnterBehaviour(_behaviourOnEnterParameters[currentState]?.Invoke()))
            {
                behaviour.Invoke();
            }
        }


        public void Tick()
        {
            if (behaviours.ContainsKey(currentState))
            {
                foreach (Action behaviour in behaviours[currentState].GetTickBehaviour(_behaviourTickParameters[currentState]?.Invoke()))
                {
                    behaviour.Invoke();
                }
            }
        }
    }
}