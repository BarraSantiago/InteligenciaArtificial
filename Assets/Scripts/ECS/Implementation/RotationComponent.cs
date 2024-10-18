using ECS.Patron;

namespace ECS.Implementation
{
    public class RotationComponent : ECSComponent
    {
        public float X;
        public float Y;
        public float Z;

        public RotationComponent(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}