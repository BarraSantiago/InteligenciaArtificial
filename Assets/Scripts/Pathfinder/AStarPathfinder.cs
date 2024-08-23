using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinder
{
    struct Transition<NodeType>
    {
        public NodeType to;
        public int cost;
        public int distance;
    }

    public class AStarPathfinder<NodeType> : Pathfinder<NodeType> where NodeType : INode<Vector2Int>, INode, new()
    {
        private Vector2IntGraph<NodeType> graph;

        private Dictionary<NodeType, List<Transition<NodeType>>> transitions =
            new Dictionary<NodeType, List<Transition<NodeType>>>();

        public AStarPathfinder(Vector2IntGraph<NodeType> graph)
        {
            this.graph = graph;
            graph.nodes.ForEach(node =>
            {
                List<Transition<NodeType>> transitionsList = new List<Transition<NodeType>>();
                
                List<NodeType> neighbors = GetNeighbors(node) as List<NodeType>;
                neighbors?.ForEach(neighbor =>
                {
                    transitionsList.Add(new Transition<NodeType>
                    {
                        to = neighbor,
                        cost = 0,
                        distance = Distance(node, neighbor)
                    });
                });
                transitions.Add(node, transitionsList);
            });
        }
        
        public AStarPathfinder(int x, int y)
        {
            graph = new Vector2IntGraph<NodeType>(x, y);
        }
        
        protected override int Distance(NodeType A, NodeType B)
        {
            int distance = 0;
        
            var aCoor = (A).GetCoordinate();
            var bCoor = (A).GetCoordinate();
        
            distance += Math.Abs(aCoor.x - bCoor.x);
            distance += Math.Abs(aCoor.y - bCoor.y);

            return distance;
        }

        protected override ICollection<NodeType> GetNeighbors(NodeType node)
        {
            List<NodeType> neighbors = new List<NodeType>();
            var nodeCoor = node.GetCoordinate();
            graph.nodes.ForEach(neighbor =>
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
            if(!GetNeighbors(A).Contains(B))
            {
                throw new InvalidOperationException("B node has to be a neighbor.");
            }
        
            int cost = 0;
        
            transitions.TryGetValue(A, out List< Transition<NodeType>> transition);

            transition?.ForEach(t =>
            {
                if (t.to.Equals(B))
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