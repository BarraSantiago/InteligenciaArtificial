using System;
using System.Collections.Generic;

namespace Pathfinder
{
    public class Node<Coordinate> : INode, INode<Coordinate>, IEquatable<Node<Coordinate>> where Coordinate : IEquatable<Coordinate>
    {
        private Coordinate coordinate;
    
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
        
        public bool EqualsTo(INode<Coordinate> other)
        {
            return coordinate.Equals(other.GetCoordinate());
        }

        public bool Equals(Node<Coordinate> other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || EqualityComparer<Coordinate>.Default.Equals(coordinate, other.coordinate);
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