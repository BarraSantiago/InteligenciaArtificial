using System.Collections.Generic;
using NeuralNetworkDirectory.ECS;
using Utils;

namespace Flocking
{
    using SimBoid = Boid<IVector, ITransform<IVector>>;

    public class FlockingManager 
    {
        public IVector Alignment(SimBoid boid)
        {
            List<SimBoid> insideRadiusBoids = EcsPopulationManager.GetBoidsInsideRadius(boid);
            if (insideRadiusBoids.Count == 0) return boid.transform.forward;

            IVector avg = MyVector.zero();
            foreach (SimBoid b in insideRadiusBoids)
            {
                avg += (IVector)b.transform.forward;
            }

            avg /= insideRadiusBoids.Count;
            return avg.Normalized();
        }

        public IVector Cohesion(SimBoid boid)
        {
            List<SimBoid> insideRadiusBoids = EcsPopulationManager.GetBoidsInsideRadius(boid);
            if (insideRadiusBoids.Count == 0) return MyVector.zero();

            IVector avg = MyVector.zero();
            foreach (SimBoid b in insideRadiusBoids)
            {
                avg += (IVector)b.transform.position;
            }

            avg /= insideRadiusBoids.Count;
            return (avg - (IVector)boid.transform.position).Normalized();
        }

        public IVector Separation(SimBoid boid)
        {
            List<SimBoid> insideRadiusBoids = EcsPopulationManager.GetBoidsInsideRadius(boid);
            if (insideRadiusBoids.Count == 0) return MyVector.zero();

            IVector avg = MyVector.zero();
            foreach (SimBoid b in insideRadiusBoids)
            {
                avg += (boid.transform.position - b.transform.position);
            }

            avg /= insideRadiusBoids.Count;
            return avg.Normalized();
        }

        public IVector Direction(SimBoid boid)
        {
            return (boid.target - boid.transform.position).Normalized();
        }
    }
}