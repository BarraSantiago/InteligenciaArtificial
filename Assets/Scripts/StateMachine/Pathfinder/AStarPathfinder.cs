using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Pathfinder
{
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
                        cost = minCost == 0 && maxCost == 0 ? 0 : RandomNumberGenerator.GetInt32(minCost, maxCost),
                    });
                });
                transitions.Add(node, transitionsList);
            });
        }

        protected override int Distance(NodeType A, NodeType B)
        {
            float distance = 0;
            Node<Vector2> nodeA = A as Node<Vector2>;
            Node<Vector2> nodeB = B as Node<Vector2>;

            distance += Math.Abs(nodeA.GetCoordinate().X - nodeB.GetCoordinate().X);
            distance += Math.Abs(nodeA.GetCoordinate().Y - nodeB.GetCoordinate().Y);

            return (int)distance;
        }

        protected override ICollection<NodeType> GetNeighbors(NodeType node)
        {
            List<NodeType> neighbors = new List<NodeType>();

            Node<Vector2> a = node as Node<Vector2>;

            var nodeCoor = a.GetCoordinate();

            Graph.ToList().ForEach(neighbor =>
            {
                var neighborNode = neighbor as Node<Vector2>;

                var neighborCoor = neighborNode.GetCoordinate();

                if ((Mathf.Approximately(neighborCoor.X, nodeCoor.X) &&
                     Mathf.Approximately(Math.Abs(neighborCoor.Y - nodeCoor.Y), 1)) ||
                    (Mathf.Approximately(neighborCoor.Y, nodeCoor.Y) &&
                     Mathf.Approximately(Math.Abs(neighborCoor.X - nodeCoor.Y), 1)) ||
                    (Mathf.Approximately(Math.Abs(neighborCoor.Y - nodeCoor.Y), 1) &&
                     Mathf.Approximately(Math.Abs(neighborCoor.X - nodeCoor.X), 1)))
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
            if (!GetNeighbors(A).Contains(B))
            {
                throw new InvalidOperationException("B node has to be a neighbor.");
            }

            int cost = 0;

            transitions.TryGetValue(A, out List<Transition<NodeType>> transition);

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
            var nodeA = A as Node<Vector2>;
            var nodeB = B as Node<Vector2>;

            return nodeA.GetCoordinate().X == nodeB.GetCoordinate().X &&
                   nodeA.GetCoordinate().Y == nodeB.GetCoordinate().Y;
        }
    }
}