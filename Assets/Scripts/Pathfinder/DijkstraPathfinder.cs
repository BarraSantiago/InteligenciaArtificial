using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinder
{
    public class DijkstraPathfinder<NodeType, TCoordinateType, TCoordinate> : Pathfinder<NodeType, TCoordinateType, TCoordinate> 
        where NodeType : INode, INode<TCoordinateType>, new()
        where TCoordinateType : IEquatable<TCoordinateType>
        where TCoordinate : ICoordinate<TCoordinateType>, new()

    { 
        public DijkstraPathfinder(ICollection<NodeType> graph)
        {
            this.Graph = graph;
        }
        
        protected override int Distance(TCoordinate A, TCoordinate B)
        {
            float distance = 0;
            Node<Vector2> nodeA = A as Node<Vector2>;
            Node<Vector2> nodeB = B as Node<Vector2>;
        
            distance += MathF.Abs(nodeA.GetCoordinate().x - nodeB.GetCoordinate().x);
            distance += MathF.Abs(nodeA.GetCoordinate().y - nodeB.GetCoordinate().y);

            return (int)distance;
        }

        protected override ICollection<INode<TCoordinateType>> GetNeighbors(NodeType node)
        {
            return node.GetNeighbors();
        }

        protected override bool IsBlocked(NodeType node)
        {
            return node.IsBlocked();
        }

        protected override int MoveToNeighborCost(NodeType A, NodeType B)
        {
            return 0;
        }

        protected override bool NodesEquals(NodeType A, NodeType B)
        {
            return Equals(A,B);
        }
    }
}
