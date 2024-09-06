using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using FlockingECS.Component;
using Utils;

namespace FlockingECS.System
{
    public class AlignmentSystem<TVector> : ECS.Patron.ECSSystem
    {
        private ParallelOptions parallelOptions;
        private IDictionary<uint, PositionComponent<TVector>> positionComponents;
        private IDictionary<uint, FlockComponent<TVector>> flockComponents;
        private IEnumerable<uint> queriedEntities;

        public override void Initialize()
        {
            parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
        }

        protected override void PreExecute(float deltaTime)
        {
            positionComponents ??= ECSManager.GetComponents<PositionComponent<TVector>>();
            flockComponents ??= ECSManager.GetComponents<FlockComponent<TVector>>();
            queriedEntities ??= ECSManager.GetEntitiesWithComponentTypes(typeof(PositionComponent<TVector>),
                typeof(VelocityComponent<TVector>));
        }

        protected override void Execute(float deltaTime)
        {
            Parallel.ForEach(queriedEntities, parallelOptions, entityId =>
            {
                var position = positionComponents[entityId];
                var flock = flockComponents[entityId];
                var insideRadiusBoids = GetBoidsInsideRadius(position);
                if (insideRadiusBoids.Count == 0) return;

                TVector avg = default;
                foreach (var b in insideRadiusBoids)
                {
                    avg = VectorHelper<TVector>.AddVectors(avg, b.Position);
                }

                avg = VectorHelper<TVector>.DivideVector(avg, insideRadiusBoids.Count);
                avg = VectorHelper<TVector>.NormalizeVector(avg);

                flock.Alignment = avg;
            });
        }

        protected override void PostExecute(float deltaTime)
        {
        }
        
        private List<PositionComponent<TVector>> GetBoidsInsideRadius(PositionComponent<TVector> boid)
        {
            List<PositionComponent<TVector>> insideRadiusBoids = new List<PositionComponent<TVector>>();
            foreach (var otherBoid in positionComponents.Values)
            {
                if (!otherBoid.Equals(boid) && VectorHelper<TVector>.IsWithinRadius(boid.Position, otherBoid.Position))
                {
                    insideRadiusBoids.Add(otherBoid);
                }
            }
            return insideRadiusBoids;
        }
    }
}