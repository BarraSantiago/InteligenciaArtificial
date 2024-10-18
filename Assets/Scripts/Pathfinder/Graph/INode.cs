using System;
using System.Collections.Generic;

namespace Pathfinder
{
    public interface INode
    {
        public bool IsBlocked();
    }

    public interface INode<Coordinate> : IEquatable<Coordinate>
        where Coordinate : IEquatable<Coordinate>
    {
        public void SetCoordinate(Coordinate coordinateType);

        public Coordinate GetCoordinate();

        public void SetNeighbors(ICollection<INode<Coordinate>> neighbors);

        public ICollection<INode<Coordinate>> GetNeighbors();

        public NodeType GetNodeType();

        public int GetCost();

        public void SetCost(int newCost);
    }
}