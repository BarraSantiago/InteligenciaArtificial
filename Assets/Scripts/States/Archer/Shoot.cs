using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace States.Archer
{
    public class Shoot : State
    {
        private const float SHOOTCOOLDOWN = 3;

        public override List<Action> GetTickBehaviour(params object[] parameters)
        {
            List<Action> behaviours = new List<Action>();

            GameObject arrowPrefab = parameters[0] as GameObject;
            Transform ownerTransform = parameters[1] as Transform;
            Transform targetTransform = parameters[2] as Transform;
            float shootForce = Convert.ToSingle(parameters[3]);
            float lastAttack = Convert.ToSingle(parameters[3]);

            behaviours.Add(() =>
            {
                if (Time.time - lastAttack > SHOOTCOOLDOWN) return;
                
                ShootArrow(arrowPrefab, ownerTransform, targetTransform, shootForce);
                
                lastAttack = Time.time;
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


        private static void ShootArrow(GameObject arrowPrefab, Transform ownerTransform, Transform targetTransform,
            float shootForce)
        {
            GameObject arrow = Helper.InstantiatePrefab(arrowPrefab, ownerTransform.position, ownerTransform.rotation);

            Vector3 direction = (targetTransform.position - ownerTransform.position).normalized;

            Rigidbody arrowRigidbody = arrow.GetComponent<Rigidbody>();

            arrowRigidbody.AddForce(direction * shootForce, ForceMode.Impulse);
        }
    }
}