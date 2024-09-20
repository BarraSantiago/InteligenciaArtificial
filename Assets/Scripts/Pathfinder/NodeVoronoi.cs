using System;
using UnityEngine;

namespace Pathfinder
{
    
    public interface ICoordinate<T> : IEquatable<T> 
        where T : IEquatable<T>
    {
        void Add(T a);
        T Multiply(float b);
        float GetX();
        float GetY();
        void SetX(float x);
        void SetY(float y);
        float Distance(T b);
        float GetMagnitude();
        T GetCoordinate();
        void SetCoordinate(float x, float y);
        void SetCoordinate(T coordinate);
        void Zero();
        void Perpendicular();
    }

    public class NodeVoronoi : ICoordinate<Vector2>, IEquatable<NodeVoronoi>
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

        public NodeVoronoi()
        {
            coordinate = Vector2.zero;
        }

        public void Add(Vector2 a)
        {
            coordinate += a;
        }
        
   

        public Vector2 Multiply(float b)
        {
            return coordinate * b;
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
        

        public Vector2 GetCoordinate()
        {
            throw new NotImplementedException();
        }

        public float Distance(Vector2 b)
        {
            return Vector2.Distance(coordinate, b);
        }

        public float GetMagnitude()
        {
            return coordinate.magnitude;
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

        public bool Equals(NodeVoronoi other)
        {
            return coordinate.Equals(other.coordinate);
        }
    }
}