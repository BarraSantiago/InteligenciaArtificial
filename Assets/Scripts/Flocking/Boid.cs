using System;
using UnityEngine;

namespace Flocking
{
    public class Boid : MonoBehaviour
    {
        public float speed = 2.5f;
        public float turnSpeed = 5f;
        public float detectionRadius = 3.0f;

        public static float alignmentWeight = 1.0f;
        public static float cohesionWeight = 1.0f;
        public static float separationWeight = 1.0f;
        public static float directionWeight = 1.0f;

        private Func<Boid, Vector3> Alignment;
        private Func<Boid, Vector3> Cohesion;
        private Func<Boid, Vector3> Separation;
        private Func<Boid, Vector3> Direction;

        public void Init(Func<Boid, Vector3> Alignment,
            Func<Boid, Vector3> Cohesion,
            Func<Boid, Vector3> Separation,
            Func<Boid, Vector3> Direction)
        {
            this.Alignment = Alignment;
            this.Cohesion = Cohesion;
            this.Separation = Separation;
            this.Direction = Direction;
        }

        private void Update()
        {
            transform.position += transform.forward * (speed * Time.deltaTime);
            transform.forward = Vector3.Lerp(transform.forward, ACS(), turnSpeed * Time.deltaTime);
        }

        public Vector3 ACS()
        {
            Vector3 ACS = Alignment(this) * alignmentWeight + Cohesion(this) * cohesionWeight +
                          Separation(this) * separationWeight + Direction(this) * directionWeight;
            return ACS.normalized;
        }
    }
}