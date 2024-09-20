using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace Pathfinder
{
    public class DepthFirstPathfinder<NodeType,TCoordinateType> : Pathfinder<NodeType,TCoordinateType> 
        where NodeType : INode<TCoordinateType>, new()
        where TCoordinateType : IEquatable<TCoordinateType>

    {
        public DepthFirstPathfinder(ICollection<NodeType> graph)
        {
            this.Graph = graph;
        }

        protected override int Distance(NodeType A, NodeType B)
        {
            int distance = 0;
            Node<Vec2Int> nodeA = A as Node<Vec2Int>;
            Node<Vec2Int> nodeB = B as Node<Vec2Int>;

            distance += Math.Abs(nodeA.GetCoordinate().x - nodeB.GetCoordinate().x);
            distance += Math.Abs(nodeA.GetCoordinate().y - nodeB.GetCoordinate().y);

            return distance;
        }

        protected override ICollection<INode<TCoordinateType>> GetNeighbors(NodeType node)
        {
            return node.GetNeighbors();
        }

        protected override bool IsBlocked(NodeType node)
        {
            return false;
        }

        protected override int MoveToNeighborCost(NodeType A, NodeType B)
        {
            return 0;
        }

        protected override bool NodesEquals(NodeType A, NodeType B)
        {
            return Equals(A, B);
        }
    }
}