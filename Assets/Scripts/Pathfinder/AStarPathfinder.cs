using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Utils;

namespace Pathfinder
{
    public class AStarPathfinder<NodeType> : Pathfinder<NodeType> where NodeType : INode<Vec2Int>, INode, new()
    {
        public AStarPathfinder(Vector2IntGraph<NodeType> graph, int minCost = -1, int maxCost = 3)
        {
            this.Graph = graph;
            
            graph.nodes.ForEach(node =>
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
        
        public AStarPathfinder(int sizeX, int sizeY)
        {
            Graph = new Vector2IntGraph<NodeType>(sizeX, sizeY);
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
            return A.GetCoordinate().x == B.GetCoordinate().x && A.GetCoordinate().y == B.GetCoordinate().y;
        }
    }
}