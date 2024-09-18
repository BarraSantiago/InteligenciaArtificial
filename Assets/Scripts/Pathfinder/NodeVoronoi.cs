using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using VoronoiDiagram;

namespace Pathfinder
{
    public interface ICoordinate<T> : IEquatable<T> where T : ICoordinate<T>
    {
        T Add(T a, T b);
        void Add(T a);
        T Subtract(T a, T b);
        T Multiply(float b);
        T Abs(T a);
        float GetX();
        float GetY();
        void SetX(float x);
        void SetY(float y);
        float Distance(T a, T b);
        float Distance(T b);
        bool IsParallel(T p1, T p2, T p3, T p4);
        T CalculateIntersection(T p1, T p2, T p3, T p4);
        float GetMagnitude(T a);
        T CalculateCenter(List<IntersectionPoint<T>> points);
        float CalculateAngle(T pos, T center);
        
        void SetCoordinate(float x, float y);
        void SetCoordinate(T coordinate);

        void Zero();
        void Perpendicular();
    }

    public class NodeVoronoi : ICoordinate<Vector2>
    {
        private Vector2 coordinate;

        public NodeVoronoi(Vector2 coordinate)
        {
            this.coordinate = coordinate;
        }

        public NodeVoronoi(int x, int y)
        {
            coordinate = new Vector2(x, y);
        }

        public Vector2 Add(Vector2 a, Vector2 b)
        {
            return a + b;
        }

        public void Add(Vector2 a)
        {
            coordinate += a;
        }

        public Vector2 Subtract(Vector2 a, Vector2 b)
        {
            return a - b;
        }

        public Vector2 Multiply(float b)
        {
            return coordinate * b;
        }

        public Vector2 Abs(Vector2 a)
        {
            return new Vector2(Mathf.Abs(a.x), Mathf.Abs(a.y));
        }

        public float GetX()
        {
            return coordinate.x;
        }

        public float GetY()
        {
            return coordinate.y;
        }

        public void SetX(float x)
        {
            coordinate.x = x;
        }

        public void SetY(float y)
        {
            coordinate.y = y;
        }

        public float Distance(Vector2 a, Vector2 b)
        {
            return Vector2.Distance(a, b);
        }

        public float Distance(Vector2 b)
        {
            return Vector2.Distance(coordinate, b);
        }

        public bool IsParallel(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            return Mathf.Approximately((p2.y - p1.y) * (p4.x - p3.x), (p4.y - p3.y) * (p2.x - p1.x));
        }

        public Vector2 CalculateIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float A1 = p2.y - p1.y;
            float B1 = p1.x - p2.x;
            float C1 = A1 * p1.x + B1 * p1.y;

            float A2 = p4.y - p3.y;
            float B2 = p3.x - p4.x;
            float C2 = A2 * p3.x + B2 * p3.y;

            float delta = A1 * B2 - A2 * B1;
            if (Mathf.Approximately(delta, 0))
            {
                return Vector2.zero; // Parallel lines
            }

            return new Vector2((B2 * C1 - B1 * C2) / delta, (A1 * C2 - A2 * C1) / delta);
        }

        public float GetMagnitude(Vector2 a)
        {
            return a.magnitude;
        }

        public Vector2 CalculateCenter(List<IntersectionPoint<Vector2>> points)
        {
            Vector2 sum = Vector2.zero;
            foreach (var point in points)
            {
                sum += point.Position;
            }

            return sum / points.Count;
        }

        public float CalculateAngle(Vector2 pos, Vector2 center)
        {
            return Mathf.Atan2(pos.y - center.y, pos.x - center.x);
        }

        public void SetCoordinate(float x, float y)
        {
            this.coordinate = new Vector2(x, y);
        }

        public void SetCoordinate(Vector2 coordinate)
        {
            this.coordinate = coordinate;
        }

        public void Zero()
        {
            coordinate = Vector2.zero;
        }

        public void Perpendicular()
        {
            coordinate = new Vector2(-coordinate.y, coordinate.x);
        }

        public bool Equals(Vector2 other)
        {
            return coordinate.Equals(other);
        }
    }
}