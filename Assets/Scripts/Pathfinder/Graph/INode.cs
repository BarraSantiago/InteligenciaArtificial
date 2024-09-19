using System;
using System.Collections.Generic;

namespace Pathfinder
{
    public interface INode
    {
        public bool IsBlocked();
        
        public ICollection<INode> GetNeighbors { get; set; }
    }

    public interface INode<Coordinate> : IEquatable<Coordinate> where Coordinate : IEquatable<Coordinate>
    {
        public void SetCoordinate(Coordinate coordinateType);
    
        public Coordinate GetCoordinate();
    }
}