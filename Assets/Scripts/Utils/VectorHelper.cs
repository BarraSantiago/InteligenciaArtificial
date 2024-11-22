using System;
using System.Collections.Generic;
using System.Numerics;

namespace Utils
{
    public class VectorHelper<TVector>
    {
        public static TVector MultiplyVector(TVector vector, float scalar)
        {
            Type type = typeof(TVector);
            if (type == typeof(Vector2))
            {
                Vector2 v = (Vector2)(object)vector;
                return (TVector)(object)(v * scalar);
            }

            if (type == typeof(Vector3))
            {
                Vector3 v = (Vector3)(object)vector;
                return (TVector)(object)(v * scalar);
            }

            throw new InvalidOperationException("Unsupported vector type");
        }

        public static TVector SubtractVectors(TVector a, TVector b)
        {
            Type type = typeof(TVector);
            if (type == typeof(Vector2))
            {
                Vector2 va = (Vector2)(object)a;
                Vector2 vb = (Vector2)(object)b;
                return (TVector)(object)(va - vb);
            }

            if (type == typeof(Vector3))
            {
                Vector3 va = (Vector3)(object)a;
                Vector3 vb = (Vector3)(object)b;
                return (TVector)(object)(va - vb);
            }

            throw new InvalidOperationException("Unsupported vector type");
        }

        public static TVector AddVectors(TVector a, TVector b, TVector c = default, TVector d = default)
        {
            Type type = typeof(TVector);
            if (type == typeof(Vector2))
            {
                Vector2 va = (Vector2)(object)a;
                Vector2 vb = (Vector2)(object)b;
                Vector2 vc = c != null ? (Vector2)(object)c : Vector2.Zero;
                Vector2 vd = d != null ? (Vector2)(object)d : Vector2.Zero;
                return (TVector)(object)(va + vb + vc + vd);
            }

            if (type == typeof(Vector3))
            {
                Vector3 va = (Vector3)(object)a;
                Vector3 vb = (Vector3)(object)b;
                Vector3 vc = c != null ? (Vector3)(object)c : Vector3.Zero;
                Vector3 vd = d != null ? (Vector3)(object)d : Vector3.Zero;
                return (TVector)(object)(va + vb + vc + vd);
            }

            throw new InvalidOperationException("Unsupported vector type");
        }

        public static TVector DivideVector(TVector vector, int scalar)
        {
            Type type = typeof(TVector);
            if (type == typeof(Vector2))
            {
                Vector2 v = (Vector2)(object)vector;
                return (TVector)(object)(v / scalar);
            }

            if (type == typeof(Vector3))
            {
                Vector3 v = (Vector3)(object)vector;
                return (TVector)(object)(v / scalar);
            }

            throw new InvalidOperationException("Unsupported vector type");
        }

        public static TVector NormalizeVector(TVector vector)
        {
            Type type = typeof(TVector);
            if (type == typeof(Vector2))
            {
                Vector2 v = (Vector2)(object)vector;
                if (v.Length() == 0) return (TVector)(object)Vector2.Zero;
                return (TVector)(object)Vector2.Normalize(v);
            }

            if (type == typeof(Vector3))
            {
                Vector3 v = (Vector3)(object)vector;
                if (v.Length() == 0) return (TVector)(object)Vector3.Zero;
                return (TVector)(object)Vector3.Normalize(v);
            }

            throw new InvalidOperationException("Unsupported vector type");
        }

        public static List<PositionComponent<TVector>> GetBoidsInsideRadius(PositionComponent<TVector> boid,
            IDictionary<uint, PositionComponent<TVector>> positionComponents)
        {
            List<PositionComponent<TVector>> insideRadiusBoids = new List<PositionComponent<TVector>>();
            foreach (PositionComponent<TVector> otherBoid in positionComponents.Values)
                if (!otherBoid.Equals(boid) && IsWithinRadius(boid.Position, otherBoid.Position))
                    insideRadiusBoids.Add(otherBoid);

            return insideRadiusBoids;
        }

        public static bool IsWithinRadius(TVector position1, TVector position2)
        {
            double distanceSquared = 0.0;
            Type type = typeof(TVector);

            if (type == typeof(Vector2))
            {
                Vector2 p1 = (Vector2)(object)position1;
                Vector2 p2 = (Vector2)(object)position2;
                distanceSquared = (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
            }
            else if (type == typeof(Vector3))
            {
                Vector3 p1 = (Vector3)(object)position1;
                Vector3 p2 = (Vector3)(object)position2;
                distanceSquared = (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y) +
                                  (p1.Z - p2.Z) * (p1.Z - p2.Z);
            }

            double radiusSquared = 3.0;
            return distanceSquared <= radiusSquared;
        }

        public static bool IsValid<TVector>(TVector newPosition)
        {
            Type type = typeof(TVector);
            if (type == typeof(Vector2))
            {
                Vector2 v = (Vector2)(object)newPosition;
                return !float.IsNaN(v.X) && !float.IsNaN(v.Y);
            }

            if (type == typeof(Vector3))
            {
                Vector3 v = (Vector3)(object)newPosition;
                return !float.IsNaN(v.X) && !float.IsNaN(v.Y) && !float.IsNaN(v.Z);
            }

            throw new InvalidOperationException("Unsupported vector type");
        }
    }
}