using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinder
{
    public class DijkstraPathfinder<NodeType> : Pathfinder<NodeType> where NodeType : INode<Vector2Int>, INode, new()
    { 
        public DijkstraPathfinder(Vector2IntGraph<NodeType> graph)
        {
            this.Graph = graph;
        }
        
        protected override int Distance(NodeType A, NodeType B)
        {
            int distance = 0;
        
            var aCoor = (A).GetCoordinate();
            var bCoor = (B).GetCoordinate();
        
            distance += Math.Abs(aCoor.x - bCoor.x);
            distance += Math.Abs(aCoor.y - bCoor.y);

            return distance;
        }

        protected override ICollection<NodeType> GetNeighbors(NodeType node)
        {
            List<NodeType> neighbors = new List<NodeType>();
            
            var nodeCoor = node.GetCoordinate();
            
            Graph.nodes.ForEach(neighbor =>
            {
                var neighborCoor = neighbor.GetCoordinate();
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
            return A.GetCoordinate().x == B.GetCoordinate().x && A.GetCoordinate().y == B.GetCoordinate().y;
        }
    }
}
