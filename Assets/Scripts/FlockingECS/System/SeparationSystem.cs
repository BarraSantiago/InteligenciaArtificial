using System.Collections.Generic;
using System.Threading.Tasks;
using ECS.Patron;
using FlockingECS.Component;
using Utils;

namespace FlockingECS.System
{
    public class SeparationSystem<TVector> : ECSSystem
    {
        private IDictionary<uint, FlockComponent<TVector>> flockComponents;
        private ParallelOptions parallelOptions;
        private IDictionary<uint, PositionComponent<TVector>> positionComponents;
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
                typeof(FlockComponent<TVector>));
        }

        protected override void Execute(float deltaTime)
        {
            Parallel.ForEach(queriedEntities, parallelOptions, entityId =>
            {
                var position = positionComponents[entityId];
                var flock = flockComponents[entityId];
                var insideRadiusBoids = VectorHelper<TVector>.GetBoidsInsideRadius(position, positionComponents);
                if (insideRadiusBoids.Count == 0) return;

                TVector separation = default;
                foreach (var b in insideRadiusBoids)
                {
                    var diff = VectorHelper<TVector>.SubtractVectors(position.Position, b.Position);
                    separation = VectorHelper<TVector>.AddVectors(separation, diff);
                }

                separation = VectorHelper<TVector>.NormalizeVector(separation);
                flock.Separation = separation;
            });
        }

        protected override void PostExecute(float deltaTime)
        {
        }
    }
}