using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace States
{
    public struct BehaviourActions
    {
        private Dictionary<int, List<Action>> mainThreadBehaviour;
        private ConcurrentDictionary<int, ConcurrentBag<Action>> multiThreadablesBehaviour;
        private Action transitionBehaviour;

        public void AddMainThreadBehaviours(int executionOrder, Action behaviour)
        {
            if (mainThreadBehaviour == null)
                mainThreadBehaviour = new Dictionary<int, List<Action>>();

            if (mainThreadBehaviour.ContainsKey(executionOrder))
            {
                mainThreadBehaviour[executionOrder].Add(behaviour);
            }
            else
            {
                mainThreadBehaviour.Add(executionOrder, new List<Action> { behaviour });
            }
        }

        public void AddMultiThreadableBehaviours(int executionOrder, Action behaviour)
        {
            if (multiThreadablesBehaviour == null)
                multiThreadablesBehaviour = new ConcurrentDictionary<int, ConcurrentBag<Action>>();

            if (multiThreadablesBehaviour.ContainsKey(executionOrder))
            {
                multiThreadablesBehaviour[executionOrder].Add(behaviour);
            }
            else
            {
                multiThreadablesBehaviour.TryAdd(executionOrder, new ConcurrentBag<Action> { behaviour });
            }
        }

        public void SetTransitionBehaviour(Action behaviour)
        {
            transitionBehaviour = behaviour;
        }

        public Dictionary<int, List<Action>> MainThreadBehaviour => mainThreadBehaviour;
        public ConcurrentDictionary<int, ConcurrentBag<Action>> MultiThreadablesBehaviour => multiThreadablesBehaviour;
        public Action TransitionBehaviour => transitionBehaviour;
    }

    public abstract class State
    {
        public Action<Enum> OnFlag;
        public abstract BehaviourActions GetTickBehaviour(params object[] parameters);
        public abstract BehaviourActions GetOnEnterBehaviour(params object[] parameters);
        public abstract BehaviourActions GetOnExitBehaviour(params object[] parameters);
    }
}