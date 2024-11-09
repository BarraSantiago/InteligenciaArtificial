using System;
using System.Collections.Generic;
using Utils;

namespace Pathfinder
{
    public enum SimNodeType
    {
        Empty,
        Blocked,
        Bush,
        Corpse,
        Carrion
    }
    public enum RTSNodeType
    {
        Empty,
        Blocked,
        Mine,
        TownCenter,
        Forest,
        Gravel
    }

    public class RTSNode<Coordinate> : INode, INode<Coordinate>, IEquatable<INode<Coordinate>>
        where Coordinate : IEquatable<Coordinate>
    {
        private Coordinate coordinate;
        private int cost;
        public int food;
        public int gold;

        private ICollection<INode<Coordinate>> neighbors;
        private SimNodeType nodeType;

        public RTSNode()
        {
        }

        public RTSNode(Coordinate coord)
        {
            coordinate = coord;
        }

        public RTSNodeType RtsNodeType { get; set; }

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

        public RTSNodeType GetRTSNodeType()
        {
            return RtsNodeType;
        }

        public SimNodeType NodeType()
        {
            return SimNodeType.Empty;
        }

        public int GetCost()
        {
            return cost;
        }

        public void SetCost(int newCost)
        {
            cost = newCost;
        }

        SimNodeType INode<Coordinate>.NodeType
        {
            get => nodeType;
            set => nodeType = value;
        }

        public int Food { get; set; }

        public bool Equals(Coordinate other)
        {
            return coordinate.Equals(other);
        }

        public bool EqualsTo(INode<Coordinate> other)
        {
            return coordinate.Equals(other.GetCoordinate());
        }

        protected bool Equals(RTSNode<Coordinate> other)
        {
            return coordinate.Equals(other.coordinate);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((RTSNode<Coordinate>)obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<Coordinate>.Default.GetHashCode(coordinate);
        }
    }
    
        public class SimNode<Coordinate> : INode, INode<Coordinate>, IEquatable<INode<Coordinate>>
        where Coordinate : IEquatable<Coordinate>
    {
        private Coordinate coordinate;
        private int cost;
        public int Food { get; set; }

        private ICollection<INode<Coordinate>> neighbors;

        public SimNode()
        {
        }

        public SimNode(Coordinate coord)
        {
            coordinate = coord;
        }
        public SimNodeType NodeType { get; set; }

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

        public RTSNodeType GetRTSNodeType()
        {
            return RTSNodeType.Empty;
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

        protected bool Equals(SimNode<Coordinate> other)
        {
            return coordinate.Equals(other.coordinate);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SimNode<Coordinate>)obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<Coordinate>.Default.GetHashCode(coordinate);
        }
    }

}