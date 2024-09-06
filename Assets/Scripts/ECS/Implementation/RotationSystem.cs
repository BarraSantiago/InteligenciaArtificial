using System.Collections.Generic;
using System.Threading.Tasks;
using ECS.Patron;

namespace ECS.Implementation
{
    public class RotationSystem : ECSSystem
    {
        private ParallelOptions parallelOptions;

        private IDictionary<uint, RotationComponent> rotationComponents;
        private IDictionary<uint, VelRotationComponent> velRotationComponents;
        private IEnumerable<uint> queryedEntities;

        public override void Initialize()
        {
            parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
        }

        protected override void PreExecute(float deltaTime)
        {
            rotationComponents??= ECSManager.GetComponents<RotationComponent>();
            velRotationComponents??= ECSManager.GetComponents<VelRotationComponent>();
            queryedEntities??= ECSManager.GetEntitiesWithComponentTypes(typeof(RotationComponent), typeof(VelRotationComponent));
        }

        protected override void Execute(float deltaTime)
        {
            Parallel.ForEach(queryedEntities, parallelOptions, i =>
            {
                rotationComponents[i].X += velRotationComponents[i].directionX * velRotationComponents[i].rotation * deltaTime;
                rotationComponents[i].Y += velRotationComponents[i].directionY * velRotationComponents[i].rotation * deltaTime;
                rotationComponents[i].Z += velRotationComponents[i].directionZ * velRotationComponents[i].rotation * deltaTime;
            });
        }

        protected override void PostExecute(float deltaTime)
        {
        }

    }
}