using System.Collections.Generic;
using UnityEngine;

public class FlockingManager : MonoBehaviour
{
    public Transform target;
    public int boidCount = 50;
    public Boid boidPrefab;
    private List<Boid> boids = new List<Boid>();

    private void Start()
    {
        for (int i = 0; i < boidCount; i++)
        {
            GameObject boidGO = Instantiate(boidPrefab.gameObject,
                new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), 0), Quaternion.identity);

            Boid boid = boidGO.GetComponent<Boid>();
            boid.Init(Alignment, Cohesion, Separation, Direction);

            boid.alignmentOffset = 1.0f;
            boid.cohesionOffset = 1.5f;
            boid.separationOffset = 2.0f;
            boid.directionOffset = 2.0f;

            boids.Add(boid);
        }
    }

    public Vector3 Alignment(Boid boid)
    {
        List<Boid> insideRadiusBoids = GetBoidsInsideRadius(boid);
        if (insideRadiusBoids.Count == 0) return boid.transform.up;

        Vector3 avg = Vector3.zero;
        foreach (Boid b in insideRadiusBoids)
        {
            avg += b.transform.up;
        }
        avg /= insideRadiusBoids.Count;
        return avg.normalized;
    }

    public Vector3 Cohesion(Boid boid)
    {
        List<Boid> insideRadiusBoids = GetBoidsInsideRadius(boid);
        if (insideRadiusBoids.Count == 0) return Vector3.zero;

        Vector3 avg = Vector3.zero;
        foreach (Boid b in insideRadiusBoids)
        {
            avg += b.transform.position;
        }
        avg /= insideRadiusBoids.Count;
        return (avg - boid.transform.position).normalized;
    }

    public Vector3 Separation(Boid boid)
    {
        List<Boid> insideRadiusBoids = GetBoidsInsideRadius(boid);
        if (insideRadiusBoids.Count == 0) return Vector3.zero;

        Vector3 avg = Vector3.zero;
        foreach (Boid b in insideRadiusBoids)
        {
            avg += (boid.transform.position - b.transform.position);
        }
        avg /= insideRadiusBoids.Count;
        return avg.normalized;
    }

    public Vector3 Direction(Boid boid)
    {
        return (target.position - boid.transform.position).normalized;
    }

    public List<Boid> GetBoidsInsideRadius(Boid boid)
    {
        List<Boid> insideRadiusBoids = new List<Boid>();

        foreach (Boid b in boids)
        {
            if (Vector3.Distance(boid.transform.position, b.transform.position) < boid.detectionRadious)
            {
                insideRadiusBoids.Add(b);
            }
        }

        return insideRadiusBoids;
    }
}