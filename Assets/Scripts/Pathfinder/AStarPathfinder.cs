using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Utils;

namespace Pathfinder
{ 
    public struct Transition<NodeType>
    {
        public NodeType to;
        public int cost;
        public int distance;
    }

    public class AStarPathfinder<NodeType> : Pathfinder<NodeType> where NodeType : INode, new()
    {
        public AStarPathfinder(ICollection<NodeType> graph, int minCost = -1, int maxCost = 3)
        {
            this.Graph = graph;
            
            graph.ToList().ForEach(node =>
            {
                List<Transition<NodeType>> transitionsList = new List<Transition<NodeType>>();
                
                List<NodeType> neighbors = GetNeighbors(node) as List<NodeType>;
                neighbors?.ForEach(neighbor =>
                {
                    transitionsList.Add(new Transition<NodeType>
                    {
                        to = neighbor,
                        cost = RandomNumberGenerator.GetInt32(minCost, maxCost),
                        distance = Distance(node, neighbor)
                    });
                });
                transitions.Add(node, transitionsList);
            });
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
                    (neighborCoor.y == nodeCoor.y && Math.Abs(neighborCoor.x - nodeCoor.x) == 1) ||
                    (Math.Abs(neighborCoor.y - nodeCoor.y) == 1 && Math.Abs(neighborCoor.x - nodeCoor.x) == 1))
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
            if(!GetNeighbors(A).Contains(B))
            {
                throw new InvalidOperationException("B node has to be a neighbor.");
            }
        
            int cost = 0;
        
            transitions.TryGetValue(A, out List< Transition<NodeType>> transition);

            transition?.ForEach(t =>
            {
                if (NodesEquals(t.to, B))
                {
                    cost = t.cost;
                }
            });
        
            return cost;
        }

        protected override bool NodesEquals(NodeType A, NodeType B)
        {
            var nodeA = A as Node<Vec2Int>;
            var nodeB = B as Node<Vec2Int>;
            
            return nodeA.GetCoordinate().x == nodeB.GetCoordinate().x && nodeA.GetCoordinate().y == nodeB.GetCoordinate().y;
        }
    }
}