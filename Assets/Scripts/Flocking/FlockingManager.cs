using System.Collections.Generic;
using NeuralNetworkDirectory.ECS;
using UnityEngine;

namespace Flocking
{
    public class FlockingManager : MonoBehaviour
    {
        public Vector2 Alignment(Boid boid)
        {
            List<Boid> insideRadiusBoids = EcsPopulationManager.GetBoidsInsideRadius(boid);
            if (insideRadiusBoids.Count == 0) return boid.transform.up;

            Vector2 avg = Vector2.zero;
            foreach (Boid b in insideRadiusBoids)
            {
                avg += (Vector2)b.transform.up;
            }

            avg /= insideRadiusBoids.Count;
            return avg.normalized;
        }

        public Vector2 Cohesion(Boid boid)
        {
            List<Boid> insideRadiusBoids = EcsPopulationManager.GetBoidsInsideRadius(boid);
            if (insideRadiusBoids.Count == 0) return Vector2.zero;

            Vector2 avg = Vector2.zero;
            foreach (Boid b in insideRadiusBoids)
            {
                avg += (Vector2)b.transform.position;
            }

            avg /= insideRadiusBoids.Count;
            return (avg - (Vector2)boid.transform.position).normalized;
        }

        public Vector2 Separation(Boid boid)
        {
            List<Boid> insideRadiusBoids = EcsPopulationManager.GetBoidsInsideRadius(boid);
            if (insideRadiusBoids.Count == 0) return Vector2.zero;

            Vector2 avg = Vector2.zero;
            foreach (Boid b in insideRadiusBoids)
            {
                avg += (Vector2)(boid.transform.position - b.transform.position);
            }

            avg /= insideRadiusBoids.Count;
            return avg.normalized;
        }

        public Vector2 Direction(Boid boid)
        {
            return (boid.target.position - boid.transform.position).normalized;
        }
    }
}