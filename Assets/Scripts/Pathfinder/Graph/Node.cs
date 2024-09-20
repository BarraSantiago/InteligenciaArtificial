using System;
using System.Collections.Generic;

namespace Pathfinder
{
    public enum NodeType
    {
        Empty,
        Blocked,
        Mine,
        TownCenter
    }

    public class Node<Coordinate> : INode, INode<Coordinate>, IEquatable<INode<Coordinate>>
        where Coordinate : IEquatable<Coordinate>
    {
        public NodeType NodeType { get; set; }
        private Coordinate coordinate;
        public int food;
        public int gold;
        public int zone;

        public void SetCoordinate(Coordinate coordinate)
        {
            this.coordinate = coordinate;
        }

        public Coordinate GetCoordinate()
        {
            return coordinate;
        }

        public bool IsBlocked()
        {
            return false;
        }

        public ICollection<INode> GetNeighbors { get; set; }

        public bool EqualsTo(INode<Coordinate> other)
        {
            return coordinate.Equals(other.GetCoordinate());
        }

        protected bool Equals(Node<Coordinate> other)
        {
            return EqualityComparer<Coordinate>.Default.Equals(coordinate, other.coordinate);
        }

        public bool Equals(Coordinate other)
        {
            return coordinate.Equals(other);
        }

        public bool Equals(INode<Coordinate> other)
        {
            return other != null && coordinate.Equals(other.GetCoordinate());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Node<Coordinate>)obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<Coordinate>.Default.GetHashCode(coordinate);
        }
    }
}