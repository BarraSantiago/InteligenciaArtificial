using System.Collections.Generic;
using System.Numerics;
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

        public DirectionSystem(PositionComponent<TVector> target)
        {
            targetPosition = new PositionComponent<TVector>(typeof(TVector) == typeof(Vector3)
                ? (TVector)(object)new Vector3(5, 5, 5)
                : (TVector)(object)new Vector2(0, 0));
        }

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