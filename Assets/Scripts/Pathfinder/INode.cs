using System;

namespace Pathfinder
{
    public interface INode
    {
        public bool IsBlocked();
    }

    public interface INode<Coordinate> : IEquatable<Coordinate> where Coordinate : IEquatable<Coordinate>
    {
        public void SetCoordinate(Coordinate coordinateType);
    
        public Coordinate GetCoordinate();
    }
}