using System;
using System.Collections.Generic;

namespace Pathfinder
{
    struct Transition<NodeType>
    {
        public NodeType to;
        public int cost;
        public int distance;
    }

    public class AStarPathfinder<NodeType> : Pathfinder<NodeType> where NodeType : INode<UnityEngine.Vector2Int>, INode, new()
    {
        private Vector2IntGraph<NodeType> graph;

        private Dictionary<NodeType, List<Transition<NodeType>>> transitions =
            new Dictionary<NodeType, List<Transition<NodeType>>>();


        protected override int Distance(NodeType A, NodeType B)
        {
            int distance = 0;
        
            var aCoor = ((INode<(int x, int y)>)A).GetCoordinate();
            var bCoor = ((INode<(int x, int y)>)A).GetCoordinate();
        
            distance += Math.Abs(aCoor.x - bCoor.x);
            distance += Math.Abs(aCoor.y - bCoor.y);

            return distance;
        }

        protected override ICollection<NodeType> GetNeighbors(NodeType node)
        {
            List<NodeType> neighbors = new List<NodeType>();
            var nodeCoor = ((INode<(int x, int y)>)node).GetCoordinate();
            graph.nodes.ForEach(neighbor =>
            {
                if (neighbor.GetCoordinate().x == nodeCoor.x && neighbor.GetCoordinate().y == nodeCoor.y + 1)
                {
                    neighbors.Add(neighbor);
                }

                if (neighbor.GetCoordinate().x == nodeCoor.x && neighbor.GetCoordinate().y == nodeCoor.y - 1)
                {
                    neighbors.Add(neighbor);
                }

                if (neighbor.GetCoordinate().x == nodeCoor.x + 1 && neighbor.GetCoordinate().y == nodeCoor.y)
                {
                    neighbors.Add(neighbor);
                }

                if (neighbor.GetCoordinate().x == nodeCoor.x - 1 && neighbor.GetCoordinate().y == nodeCoor.y)
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
            return A.Equals(B);
        }
    }
}