using System;

namespace Utils
{
    public class IVector : IEquatable<IVector>
    {
        public float x;
        public float y;

        private IVector(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public IVector zero()
        {
            return new IVector(0, 0);
        }
        
        public float Distance(IVector coordinate)
        {
            return (float) System.Math.Sqrt(System.Math.Pow(coordinate.x - x, 2) + System.Math.Pow(coordinate.y - y, 2));
        }

        public bool Equals(IVector other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((IVector)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }
    }
}