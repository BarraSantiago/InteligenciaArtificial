using System.Collections.Generic;
using System.Threading.Tasks;
using ECS.Patron;
using FlockingECS.Component;
using Utils;

namespace FlockingECS.System
{
    public class MoveSystem<TVector> : ECSSystem
    {
        private IDictionary<uint, FlockComponent<TVector>> flockComponents;
        private OffsetComponent offsetComponent;
        private ParallelOptions parallelOptions;
        private IDictionary<uint, PositionComponent<TVector>> positionComponents;
        private IEnumerable<uint> queriedEntities;

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
                typeof(FlockComponent<TVector>));
        }

        protected override void Execute(float deltaTime)
        {
            Parallel.ForEach(queriedEntities, parallelOptions, i =>
            {
                var alignment = Multiply(flockComponents[i].Alignment, offsetComponent.alignmentWeight);
                var cohesion = Multiply(flockComponents[i].Cohesion, offsetComponent.cohesionWeight);
                var separation = Multiply(flockComponents[i].Separation, offsetComponent.separationWeight);
                var direction = Multiply(flockComponents[i].Direction, offsetComponent.directionWeight);
                var ACS = VectorHelper<TVector>.AddVectors(alignment, cohesion, separation, direction);

                ACS = VectorHelper<TVector>.NormalizeVector(ACS);


                positionComponents[i].Position = VectorHelper<TVector>.AddVectors(positionComponents[i].Position,
                    Multiply(ACS, offsetComponent.speed * deltaTime));
            });
        }

        private TVector Multiply(TVector vector, float weight)
        {
            return VectorHelper<TVector>.MultiplyVector(vector, weight);
        }

        protected override void PostExecute(float deltaTime)
        {
        }
    }
}