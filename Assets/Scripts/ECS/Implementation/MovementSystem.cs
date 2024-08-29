using System.Collections.Generic;
using System.Threading.Tasks;

public sealed class MovementSystem : ECSSystem
{
    private ParallelOptions parallelOptions;

    private IDictionary<uint, PositionComponent> positionComponents;
    private IDictionary<uint, VelocityComponent> velocityComponents;
    private IEnumerable<uint> queryedEntities;

    public override void Initialize()
    {
        parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
    }

    protected override void PreExecute(float deltaTime)
    {
        positionComponents??= ECSManager.GetComponents<PositionComponent>();
        velocityComponents??= ECSManager.GetComponents<VelocityComponent>();
        queryedEntities??= ECSManager.GetEntitiesWhitComponentTypes(typeof(PositionComponent), typeof(VelocityComponent));
    }

    protected override void Execute(float deltaTime)
    {
        Parallel.ForEach(queryedEntities, parallelOptions, i =>
        {
            positionComponents[i].X += velocityComponents[i].directionX * velocityComponents[i].velocity * deltaTime;
            positionComponents[i].Y += velocityComponents[i].directionY * velocityComponents[i].velocity * deltaTime;
            positionComponents[i].Z += velocityComponents[i].directionZ * velocityComponents[i].velocity * deltaTime;
        });
    }

    protected override void PostExecute(float deltaTime)
    {
    }
}
