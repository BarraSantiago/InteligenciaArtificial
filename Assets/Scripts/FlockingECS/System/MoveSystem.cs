using System;
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
                typeof(FlockComponent<TVector>));
        }

        protected override void Execute(float deltaTime)
        {
            Parallel.ForEach(queriedEntities, parallelOptions, i =>
            {
                TVector alignment =
                    VectorHelper<TVector>.MultiplyVector(flockComponents[i].Alignment, offsetComponent.alignmentWeight);
                if (!VectorHelper<TVector>.IsValid(alignment))
                {
                    throw new Exception($"Invalid alignment vector for entity {i}");
                }

                TVector cohesion =
                    VectorHelper<TVector>.MultiplyVector(flockComponents[i].Cohesion, offsetComponent.cohesionWeight);
                if (!VectorHelper<TVector>.IsValid(cohesion))
                {
                    throw new Exception($"Invalid cohesion vector for entity {i}");
                }

                TVector separation = VectorHelper<TVector>.MultiplyVector(flockComponents[i].Separation,
                    offsetComponent.separationWeight);
                if (!VectorHelper<TVector>.IsValid(separation))
                {
                    throw new Exception($"Invalid separation vector for entity {i}");
                }

                TVector direction =
                    VectorHelper<TVector>.MultiplyVector(flockComponents[i].Direction, offsetComponent.directionWeight);
                if (!VectorHelper<TVector>.IsValid(direction))
                {
                    throw new Exception($"Invalid direction vector for entity {i}");
                }

                TVector ACS = VectorHelper<TVector>.AddVectors(
                    VectorHelper<TVector>.AddVectors(VectorHelper<TVector>.AddVectors(alignment, cohesion), separation),
                    direction);
                if (!VectorHelper<TVector>.IsValid(ACS))
                {
                    throw new Exception($"Invalid ACS vector for entity {i}");
                }

                ACS = VectorHelper<TVector>.NormalizeVector(ACS);
                if (!VectorHelper<TVector>.IsValid(ACS))
                {
                    throw new Exception($"Invalid normalized ACS vector for entity {i}");
                }

                TVector newPosition = VectorHelper<TVector>.AddVectors(positionComponents[i].Position,
                    VectorHelper<TVector>.MultiplyVector(ACS, offsetComponent.speed * deltaTime));
                if (VectorHelper<TVector>.IsValid(newPosition))
                {
                    positionComponents[i].Position = newPosition;
                }
                else
                {
                    throw new Exception($"Invalid position calculated for entity {i}");
                    // Handle the invalid position (e.g., reset to a valid position)
                }
            });
        }

        protected override void PostExecute(float deltaTime)
        {
        }
    }
}