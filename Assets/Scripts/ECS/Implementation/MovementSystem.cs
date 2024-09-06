using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using ECS.Patron;

public sealed class MovementSystem : ECSSystem
{
    private ParallelOptions parallelOptions;

    private IDictionary<uint, PositionComponent<Vector3>> positionComponents;
    private IDictionary<uint, VelocityComponent<Vector3>> velocityComponents;
    private IEnumerable<uint> queriedEntities;

    public override void Initialize()
    {
        parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
    }

    protected override void PreExecute(float deltaTime)
    {
        positionComponents??= ECSManager.GetComponents<PositionComponent<Vector3>>();
        velocityComponents??= ECSManager.GetComponents<VelocityComponent<Vector3>>();
        queriedEntities??= ECSManager.GetEntitiesWithComponentTypes(typeof(PositionComponent<Vector3>), typeof(VelocityComponent<Vector3>));
    }

    protected override void Execute(float deltaTime)
    {
        Parallel.ForEach(queriedEntities, parallelOptions, i =>
        {
            positionComponents[i].Position.X += velocityComponents[i].direction.X * velocityComponents[i].velocity * deltaTime;
            positionComponents[i].Position.Y += velocityComponents[i].direction.Y * velocityComponents[i].velocity * deltaTime;
            positionComponents[i].Position.Z += velocityComponents[i].direction.Z * velocityComponents[i].velocity * deltaTime;
        });
    }

    protected override void PostExecute(float deltaTime)
    {
    }
}
