using System;
using System.Collections.Generic;
using Units;
using UnityEngine;

namespace States.Generic
{
    public sealed class PatrolState : State
    {
        private bool direction;

        public override List<Action> GetTickBehaviour(params object[] parameters)
        {
            List<Action> behaviours = new List<Action>();

            Transform ownerTransform = parameters[0] as Transform;
            Transform wayPoint1 = parameters[1] as Transform;
            Transform wayPoint2 = parameters[2] as Transform;
            Transform chaseTarget = parameters[3] as Transform;
            float speed = Convert.ToSingle(parameters[4]);
            float chaseDistance = Convert.ToSingle(parameters[5]);

            behaviours.Add(() =>
            {
                if (Vector3.Distance(ownerTransform.position, direction ? wayPoint1.position : wayPoint2.position) < 0.2f)
                {
                    direction = !direction;
                }

                ownerTransform.position +=
                    (direction ? wayPoint1.position : wayPoint2.position - ownerTransform.position).normalized * (speed * Time.deltaTime);
            });

            behaviours.Add(() =>
            {
                if (Vector3.Distance(ownerTransform.position, chaseTarget.position) < chaseDistance)
                {
                    OnFlag?.Invoke((int)Flags.OnTargetNear);
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