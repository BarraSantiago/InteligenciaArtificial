using System;
using Pathfinder;
using UnityEngine;

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
        return coordinate;
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
        coordinate = new Vector2(x, y);
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