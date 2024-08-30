using System;
using Units;
using UnityEngine;

namespace States.Generic
{
    public sealed class PatrolState : State
    {
        private bool direction;

        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            Transform ownerTransform = parameters[0] as Transform;
            Transform wayPoint1 = parameters[1] as Transform;
            Transform wayPoint2 = parameters[2] as Transform;
            Transform chaseTarget = parameters[3] as Transform;
            float speed = Convert.ToSingle(parameters[4]);
            float chaseDistance = Convert.ToSingle(parameters[5]);

            behaviours.AddMainThreadBehaviours(0, () =>
            {
                if (Vector3.Distance(ownerTransform.position, direction ? wayPoint1.position : wayPoint2.position) <
                    0.2f)
                {
                    direction = !direction;
                }

                ownerTransform.position +=
                    (direction ? wayPoint1.position : wayPoint2.position - ownerTransform.position).normalized *
                    (speed * Time.deltaTime);
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (Vector3.Distance(ownerTransform.position, chaseTarget.position) < chaseDistance)
                {
                    OnFlag?.Invoke(Flags.OnTargetNear);
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