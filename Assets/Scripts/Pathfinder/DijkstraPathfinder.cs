using System;
using System.Collections.Generic;
using StateMachine.Agents.RTS;

namespace Pathfinder
{
    public class
        DijkstraPathfinder<NodeType, TCoordinateType, TCoordinate> : Pathfinder<NodeType, TCoordinateType, TCoordinate>
        where NodeType : INode, INode<TCoordinateType>, new()
        where TCoordinateType : IEquatable<TCoordinateType>
        where TCoordinate : ICoordinate<TCoordinateType>, new()

    {
        public DijkstraPathfinder(ICollection<NodeType> graph)
        {
            Graph = graph;
        }

        protected override int Distance(TCoordinate A, TCoordinate B)
        {
            if (A == null || B == null) return int.MaxValue;

            float distance = 0;

            distance += Math.Abs(A.GetX() - B.GetX());
            distance += Math.Abs(A.GetY() - B.GetY());

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
            return Equals(A, B);
        }
    }
}