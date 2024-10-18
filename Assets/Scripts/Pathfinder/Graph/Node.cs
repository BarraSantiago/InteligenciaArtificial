using System;
using System.Collections.Generic;

namespace Pathfinder
{
    public enum NodeType
    {
        Empty,
        Blocked,
        Mine,
        TownCenter,
        Forest,
        Gravel
    }

    public class Node<Coordinate> : INode, INode<Coordinate>, IEquatable<INode<Coordinate>>
        where Coordinate : IEquatable<Coordinate>
    {
        private Coordinate coordinate;
        private int cost;
        public int food;
        public int gold;

        private ICollection<INode<Coordinate>> neighbors;

        public Node()
        {
        }

        public Node(Coordinate coord)
        {
            coordinate = coord;
        }

        public NodeType NodeType { get; set; }

        public bool Equals(INode<Coordinate> other)
        {
            return other != null && coordinate.Equals(other.GetCoordinate());
        }

        public bool IsBlocked()
        {
            return false;
        }

        public void SetCoordinate(Coordinate coordinate)
        {
            this.coordinate = coordinate;
        }

        public Coordinate GetCoordinate()
        {
            return coordinate;
        }

        public void SetNeighbors(ICollection<INode<Coordinate>> neighbors)
        {
            this.neighbors = neighbors;
        }

        public ICollection<INode<Coordinate>> GetNeighbors()
        {
            return neighbors;
        }

        public NodeType GetNodeType()
        {
            return NodeType;
        }

        public int GetCost()
        {
            return cost;
        }

        public void SetCost(int newCost)
        {
            cost = newCost;
        }

        public bool Equals(Coordinate other)
        {
            return coordinate.Equals(other);
        }

        public bool EqualsTo(INode<Coordinate> other)
        {
            return coordinate.Equals(other.GetCoordinate());
        }

        protected bool Equals(Node<Coordinate> other)
        {
            return coordinate.Equals(other.coordinate);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Node<Coordinate>)obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<Coordinate>.Default.GetHashCode(coordinate);
        }
    }
}