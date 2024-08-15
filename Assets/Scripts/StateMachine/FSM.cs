using System;
using System.Collections.Generic;

namespace StateMachine
{
    public class FSM<EnumState, EnumFlag>
        where EnumState : Enum
        where EnumFlag : Enum
    {
        private const int UNNASIGNED_TRANSITION = -1;
        private int _currentState = 0;
        private readonly Dictionary<int, State> _behaviours;
        private readonly Dictionary<int, Func<object[]>> _behaviourTickParameters;
        private readonly Dictionary<int, Func<object[]>> _behaviourOnEnterParameters;
        private readonly Dictionary<int, Func<object[]>> _behaviourOnExitParameters;
        private readonly int[,] _transitions;


        public FSM()
        {
            int states = Enum.GetValues(typeof(EnumState)).Length;
            int flags = Enum.GetValues(typeof(EnumFlag)).Length;
            _behaviours = new Dictionary<int, State>();
            _transitions = new int[states, flags];

            for (int i = 0; i < states; i++)
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

        public void ForceTransition(Enum state)
        {
            _currentState = Convert.ToInt32(state);
        }

        public void AddBehaviour<T>(EnumState stateIndexEnum, Func<object[]> onTickParameters = null,
            Func<object[]> onEnterParameters = null, Func<object[]> onExitParameters = null) where T : State, new()
        {
            int stateIndex = Convert.ToInt32(stateIndexEnum);
            if (_behaviours.ContainsKey(stateIndex)) return;

            State newBehaviour = new T();
            newBehaviour.OnFlag += Transition;
            _behaviours.Add(stateIndex, newBehaviour);
            _behaviourTickParameters.Add(stateIndex, onTickParameters);
            _behaviourOnEnterParameters.Add(stateIndex, onEnterParameters);
            _behaviourOnExitParameters.Add(stateIndex, onExitParameters);
        }

        public void SetTransition(Enum originState, Enum flag, Enum destinationState)
        {
            _transitions[Convert.ToInt32(originState), Convert.ToInt32(flag)] = Convert.ToInt32(destinationState);
        }

        private void Transition(Enum flag)
        {
            if (_transitions[_currentState, Convert.ToInt32(flag)] == UNNASIGNED_TRANSITION) return;

            foreach (Action behaviour in _behaviours[_currentState]
                         .GetOnExitBehaviour(_behaviourOnEnterParameters[_currentState]?.Invoke()))
            {
                behaviour.Invoke();
            }

            _currentState = _transitions[_currentState, Convert.ToInt32(flag)];
            
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