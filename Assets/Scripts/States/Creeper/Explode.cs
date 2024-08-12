using System;
using System.Collections.Generic;
using UnityEngine;

namespace States.Creeper
{
    public sealed class ExplodeState : State
    {
        public override List<Action> GetTickBehaviour(params object[] parameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() => { Debug.Log("F"); });
        
            GameObject ownerObject = parameters[0] as GameObject;
        
            behaviours.Add(() =>
            {
                ownerObject.SetActive( false);
            });
        
            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviour(params object[] parameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() => { Debug.Log("Explode!"); });
        
        
            return behaviours;
        }

        public override List<Action> GetOnExitBehaviour(params object[] parameters)
        {
            return new List<Action>();
        }
    }
}