using System;
using Units;
using UnityEngine;

namespace States.Generic
{
    public sealed class ChaseState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            var ownerTransform = parameters[0] as Transform;
            var targetTransform = parameters[1] as Transform;
            var speed = Convert.ToSingle(parameters[2]);
            var reachDistance = Convert.ToSingle(parameters[3]);
            var lostDistance = Convert.ToSingle(parameters[4]);

            var behaviours = new BehaviourActions();

            if (!ownerTransform && !targetTransform) return default;

            behaviours.AddMainThreadBehaviours(0, () =>
            {
                ownerTransform.position += (targetTransform.position - ownerTransform.position).normalized *
                                           (speed * Time.deltaTime);
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (Vector3.Distance(targetTransform.position, ownerTransform.position) > lostDistance)
                {
                    OnFlag?.Invoke(Flags.OnTargetLost);
                    return;
                }

                if (Vector3.Distance(targetTransform.position, ownerTransform.position) < reachDistance)
                {
                    OnFlag?.Invoke(Flags.OnTargetReach);
                }
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