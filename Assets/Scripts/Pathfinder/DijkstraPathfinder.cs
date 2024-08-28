using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace Pathfinder
{
    public class DijkstraPathfinder<NodeType> : Pathfinder<NodeType> where NodeType : INode, new()
    { 
        public DijkstraPathfinder(ICollection<NodeType> graph)
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

        protected override ICollection<NodeType> GetNeighbors(NodeType node)
        {
            List<NodeType> neighbors = new List<NodeType>();

            Node<Vec2Int> a = node as Node<Vec2Int>;
            
            var nodeCoor = a.GetCoordinate();
            
            Graph.ToList().ForEach(neighbor =>
            {
                var neighborNode = neighbor as Node<Vec2Int>;
                
                var neighborCoor = neighborNode.GetCoordinate();
                
                if ((neighborCoor.x == nodeCoor.x && Math.Abs(neighborCoor.y - nodeCoor.y) == 1) ||
                    (neighborCoor.y == nodeCoor.y && Math.Abs(neighborCoor.x - nodeCoor.x) == 1))
                {
                    neighbors.Add(neighbor);
                }
            });

            return neighbors;
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
