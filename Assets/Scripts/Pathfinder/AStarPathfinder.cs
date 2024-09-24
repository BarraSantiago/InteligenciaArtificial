using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Pathfinder
{
    public class AStarPathfinder<NodeType, CoordinateType, TCoordinate> : Pathfinder<NodeType, CoordinateType, TCoordinate>
        where NodeType : INode, INode<CoordinateType>, new()
        where CoordinateType : IEquatable<CoordinateType>
        where TCoordinate : ICoordinate<CoordinateType>, new()
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

        protected override int Distance(TCoordinate A, TCoordinate B)
        {
            if (A == null || B == null)
            {
                return int.MaxValue;
            }

            float distance = 0;

            distance += Math.Abs(A.GetX() - B.GetX());
            distance += Math.Abs(A.GetY() - B.GetY());

            return (int)distance;
        }

        protected override ICollection<INode<CoordinateType>> GetNeighbors(NodeType node)
        {
            return node.GetNeighbors();
        }

        public bool Approximately(float a, float b)
        {
            return Math.Abs(a - b) < 1e-6f;
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
            if (A == null || B == null)
            {
                return false;
            } 
            
            return A.Equals(B);
        }
    }
}