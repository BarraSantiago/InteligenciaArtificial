using System.Collections.Generic;
using System.Threading.Tasks;
using ECS.Patron;
using FlockingECS.Component;
using Utils;

namespace FlockingECS.System
{
    public class MoveSystem<TVector> : ECSSystem
    {
        private ParallelOptions parallelOptions;
        private IDictionary<uint, PositionComponent<TVector>> positionComponents;
        private IDictionary<uint, FlockComponent<TVector>> flockComponents;
        private IEnumerable<uint> queriedEntities;
        private PositionComponent<TVector> targetPosition;

        private OffsetComponent offsetComponent;

        public override void Initialize()
        {
            parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
            offsetComponent = new OffsetComponent(1, 1, 2, 1.5f);
        }

        protected override void PreExecute(float deltaTime)
        {
            positionComponents ??= ECSManager.GetComponents<PositionComponent<TVector>>();
            flockComponents ??= ECSManager.GetComponents<FlockComponent<TVector>>();
            queriedEntities ??= ECSManager.GetEntitiesWithComponentTypes(typeof(PositionComponent<TVector>),
                typeof(VelocityComponent<TVector>), typeof(FlockComponent<TVector>));
        }

        protected override void Execute(float deltaTime)
        {
            Parallel.ForEach(queriedEntities, parallelOptions, i =>
            {
                TVector alignment = VectorHelper<TVector>.MultiplyVector(flockComponents[i].Alignment, offsetComponent.alignmentWeight);
                TVector cohesion = VectorHelper<TVector>.MultiplyVector(flockComponents[i].Cohesion, offsetComponent.cohesionWeight);
                TVector separation = VectorHelper<TVector>.MultiplyVector(flockComponents[i].Separation, offsetComponent.separationWeight);
                TVector direction = VectorHelper<TVector>.MultiplyVector(flockComponents[i].Direction, offsetComponent.directionWeight);
                TVector ACS = VectorHelper<TVector>.AddVectors(VectorHelper<TVector>.AddVectors(VectorHelper<TVector>.AddVectors(alignment, cohesion), separation), direction);

                ACS = VectorHelper<TVector>.NormalizeVector(ACS);
                
                positionComponents[i].Position = VectorHelper<TVector>.AddVectors(positionComponents[i].Position, VectorHelper<TVector>.MultiplyVector(ACS, offsetComponent.speed * deltaTime));
                
            });
        }

        protected override void PostExecute(float deltaTime)
        {
        }
    }
}