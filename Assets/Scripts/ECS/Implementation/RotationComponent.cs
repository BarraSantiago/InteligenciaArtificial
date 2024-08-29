namespace ECS.Implementation
{
    public class RotationComponent : ECSComponent
    {
        public float X;
        public float Y;
        public float Z;

        public RotationComponent(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

    }
}