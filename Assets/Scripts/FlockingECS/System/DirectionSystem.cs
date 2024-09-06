using System.Collections.Generic;
using System.Threading.Tasks;
using ECS.Patron;
using FlockingECS.Component;
using Utils;

namespace FlockingECS.System
{
    public class DirectionSystem<TVector> : ECSSystem
    {
        private ParallelOptions parallelOptions;
        private IDictionary<uint, PositionComponent<TVector>> positionComponents;
        private IDictionary<uint, FlockComponent<TVector>> flockComponents;
        private IEnumerable<uint> queriedEntities;
        private PositionComponent<TVector> targetPosition;

        public override void Initialize()
        {
            parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
        }

        protected override void PreExecute(float deltaTime)
        {
            positionComponents ??= ECSManager.GetComponents<PositionComponent<TVector>>();
            flockComponents ??= ECSManager.GetComponents<FlockComponent<TVector>>();
            queriedEntities ??= ECSManager.GetEntitiesWithComponentTypes(typeof(PositionComponent<TVector>),
                typeof(VelocityComponent<TVector>), typeof(FlockComponent<TVector>));

            // TODO FIX THIS
            targetPosition = ECSManager.GetComponent<PositionComponent<TVector>>(0);
        }

        protected override void Execute(float deltaTime)
        {
            Parallel.ForEach(queriedEntities, parallelOptions, entityId =>
            {
                var position = positionComponents[entityId];
                var flock = flockComponents[entityId];

                TVector direction = VectorHelper<TVector>.SubtractVectors(targetPosition.Position, position.Position);
                direction = VectorHelper<TVector>.NormalizeVector(direction);

                flock.Direction = direction;
            });
        }

        protected override void PostExecute(float deltaTime)
        {
        }
    }
}