using System;
using System.Collections.Generic;
using Units;
using UnityEngine;

namespace States.Generic
{
    public sealed class ChaseState : State
    {
        public override List<Action> GetTickBehaviour(params object[] parameters)
        {
            Transform ownerTransform = parameters[0] as Transform;
            Transform targetTransform = parameters[1] as Transform;
            float speed = Convert.ToSingle(parameters[2]);
            float explodeDistance = Convert.ToSingle(parameters[3]);
            float lostDistance = Convert.ToSingle(parameters[4]);

            List<Action> behaviours = new List<Action>();

            if (!ownerTransform && !targetTransform) return null;
            behaviours.Add(() =>
            {
                ownerTransform.position += (targetTransform.position - ownerTransform.position).normalized *
                                           (speed * Time.deltaTime);
            });

            behaviours.Add(() =>
            {
                if (Vector3.Distance(targetTransform.position, ownerTransform.position) < explodeDistance)
                {
                    OnFlag?.Invoke((int)Flags.OnTargetReach);
                }
            });

            behaviours.Add(() =>
            {
                if (Vector3.Distance(targetTransform.position, ownerTransform.position) > lostDistance)
                {
                    OnFlag?.Invoke((int)Flags.OnTargetLost);
                }
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviour(params object[] parameters)
        {
            return new List<Action>();
        }

        public override List<Action> GetOnExitBehaviour(params object[] parameters)
        {
            return new List<Action>();
        }
    }
}