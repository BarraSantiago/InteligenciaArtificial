using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Flocking
{
    public class FlockingManager : MonoBehaviour
    {
        public Transform target;
        public int boidCount = 50;
        public Boid boidPrefab;
        [SerializeField] private float alignmentWeight = 1.0f;
        [SerializeField] private float cohesionWeight = 1.0f;
        [SerializeField] private float separationWeight = 1.0f;
        [SerializeField] private float directionWeight = 1.0f;

        private readonly List<Boid> boids = new();

        private void Start()
        {
            for (var i = 0; i < boidCount; i++)
            {
                var boidGO = Instantiate(boidPrefab.gameObject,
                    new Vector3(Random.Range(-10, 10), Random.Range(-10, 10)), Quaternion.identity);

                var boid = boidGO.GetComponent<Boid>();
                boid.Init(Alignment, Cohesion, Separation, Direction);
                boids.Add(boid);
            }
        }


        private void OnValidate()
        {
            Boid.alignmentWeight = alignmentWeight;
            Boid.cohesionWeight = cohesionWeight;
            Boid.separationWeight = separationWeight;
            Boid.directionWeight = directionWeight;
        }

        public Vector3 Alignment(Boid boid)
        {
            var insideRadiusBoids = GetBoidsInsideRadius(boid);
            if (insideRadiusBoids.Count == 0) return Vector3.zero;

            var avg = Vector3.zero;
            foreach (var b in insideRadiusBoids) avg += b.transform.up;

            avg /= insideRadiusBoids.Count;
            return avg.normalized;
        }

        public Vector3 Cohesion(Boid boid)
        {
            var insideRadiusBoids = GetBoidsInsideRadius(boid);
            if (insideRadiusBoids.Count == 0) return Vector3.zero;

            var avg = Vector3.zero;
            foreach (var b in insideRadiusBoids) avg += b.transform.position;

            avg /= insideRadiusBoids.Count;
            return (avg - boid.transform.position).normalized;
        }

        public Vector3 Separation(Boid boid)
        {
            var insideRadiusBoids = GetBoidsInsideRadius(boid);
            if (insideRadiusBoids.Count == 0) return Vector3.zero;

            var avg = Vector3.zero;
            foreach (var b in insideRadiusBoids) avg += boid.transform.position - b.transform.position;

            avg /= insideRadiusBoids.Count;
            return avg.normalized;
        }

        public Vector3 Direction(Boid boid)
        {
            return (target.position - boid.transform.position).normalized;
        }

        public List<Boid> GetBoidsInsideRadius(Boid boid)
        {
            var insideRadiusBoids = new List<Boid>();

            foreach (var b in boids)
                if (Vector3.Distance(boid.transform.position, b.transform.position) < boid.detectionRadius)
                    insideRadiusBoids.Add(b);

            return insideRadiusBoids;
        }
    }
}