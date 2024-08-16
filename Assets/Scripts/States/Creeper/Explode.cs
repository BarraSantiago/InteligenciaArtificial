using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace States.Creeper
{
    public sealed class ExplodeState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();
        
            GameObject ownerObject = parameters[0] as GameObject;
        
            behaviours.AddMainThreadBehaviours(1,() =>
            {
                ownerObject.SetActive( false);
            });
        
            return behaviours;
        }

        public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();
            behaviours.AddMultiThreadableBehaviours(0, () => { Debug.Log("Explode!"); });
        
        
            return behaviours;
        }

        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            return default;
        }
    }
}