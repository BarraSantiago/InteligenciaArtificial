using System;
using System.Collections.Generic;
using UnityEngine;

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
        public NodeType NodeType { get; set; }
        public int food;
        public int gold;
        
        private ICollection<INode<Coordinate>> neighbors;
        private Coordinate coordinate;
        private int cost;

        public Node()
        {
        }
        public Node(Coordinate coord)
        {
            coordinate = coord;
        }

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

        public void SetNeighbors(ICollection<INode<Coordinate>> neighbors)
        {
            this.neighbors = neighbors;
        }

        public ICollection<INode<Coordinate>> GetNeighbors()
        {
            return neighbors as ICollection<INode<Coordinate>>;
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

        public bool EqualsTo(INode<Coordinate> other)
        {
            return coordinate.Equals(other.GetCoordinate());
        }

        protected bool Equals(Node<Coordinate> other)
        {
            return coordinate.Equals(other.coordinate);
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