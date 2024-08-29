namespace ECS.Implementation
{
    public class VelRotationComponent : ECSComponent
    {
        public float rotation;

        public float directionX;
        public float directionY;
        public float directionZ;

        public VelRotationComponent(float rotation, float directionX, float directionY, float directionZ)
        {
            this.rotation = rotation;
            this.directionX = directionX;
            this.directionY = directionY;
            this.directionZ = directionZ;
        }

    }
}