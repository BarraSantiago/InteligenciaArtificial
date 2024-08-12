using System;
using System.Collections.Generic;

namespace StateMachine
{
    public class FSM
    {
        private const int UNNASIGNED_TRANSITION = -1;
        private int _currentState = 0;
        private readonly Dictionary<int, State> _behaviours;
        private readonly Dictionary<int, Func<object[]>> _behaviourTickParameters;
        private readonly Dictionary<int, Func<object[]>> _behaviourOnEnterParameters;
        private readonly Dictionary<int, Func<object[]>> _behaviourOnExitParameters;
        private readonly int[,] _transitions;


        public FSM(int statesAmount, int flags)
        {
            _behaviours = new Dictionary<int, State>();
            _transitions = new int[statesAmount, flags];

            for (int i = 0; i < statesAmount; i++)
            {
                for (int j = 0; j < flags; j++)
                {
                    _transitions[i, j] = UNNASIGNED_TRANSITION;
                }
            }

            _behaviourTickParameters = new Dictionary<int, Func<object[]>>();
            _behaviourOnEnterParameters = new Dictionary<int, Func<object[]>>();
            _behaviourOnExitParameters = new Dictionary<int, Func<object[]>>();
        }

        public void Force(int state)
        {
            _currentState = state;
        }

        public void AddBehaviour<T>(int stateIndex, Func<object[]> onTickParameters = null,
            Func<object[]> onEnterParameters = null, Func<object[]> onExitParameters = null) where T : State, new()
        {
            if (_behaviours.ContainsKey(stateIndex)) return;

            State newBehaviour = new T();
            newBehaviour.OnFlag += Transition;
            _behaviours.Add(stateIndex, newBehaviour);
            _behaviourTickParameters.Add(stateIndex, onTickParameters);
            _behaviourOnEnterParameters.Add(stateIndex, onEnterParameters);
            _behaviourOnExitParameters.Add(stateIndex, onExitParameters);
        }

        public void SetTransition(int originState, int flag, int destinationState)
        {
            _transitions[originState, flag] = destinationState;
        }

        private void Transition(int flag)
        {
            foreach (Action behaviour in _behaviours[_currentState]
                         .GetOnExitBehaviour(_behaviourOnEnterParameters[_currentState]?.Invoke()))
            {
                behaviour.Invoke();
            }

            _currentState = _transitions[_currentState, flag];
            foreach (Action behaviour in _behaviours[_currentState]
                         .GetOnEnterBehaviour(_behaviourOnEnterParameters[_currentState]?.Invoke()))
            {
                behaviour.Invoke();
            }
        }


        public void Tick()
        {
            if (!_behaviours.ContainsKey(_currentState)) return;
            
            foreach (Action behaviour in _behaviours[_currentState]
                         .GetTickBehaviour(_behaviourTickParameters[_currentState]?.Invoke()))
            {
                behaviour.Invoke();
            }
        }
    }
}