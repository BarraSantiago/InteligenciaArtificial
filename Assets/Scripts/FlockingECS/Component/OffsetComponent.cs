using ECS.Patron;

namespace FlockingECS.Component
{
    public class OffsetComponent : ECSComponent
    {
        public float alignmentWeight = 1.0f;
        public float cohesionWeight = 1.0f;
        public float directionWeight = 1.0f;
        public float separationWeight = 1.0f;
        public float speed = 1;

        public OffsetComponent(float alignmentWeight, float cohesionWeight, float separationWeight,
            float directionWeight)
        {
            this.alignmentWeight = alignmentWeight;
            this.cohesionWeight = cohesionWeight;
            this.separationWeight = separationWeight;
            this.directionWeight = directionWeight;
        }
    }
}