using ECS.Patron;

namespace ECS.Implementation
{
    public class VelRotationComponent : ECSComponent
    {
        public float directionX;
        public float directionY;
        public float directionZ;
        public float rotation;

        public VelRotationComponent(float rotation, float directionX, float directionY, float directionZ)
        {
            this.rotation = rotation;
            this.directionX = directionX;
            this.directionY = directionY;
            this.directionZ = directionZ;
        }
    }
}