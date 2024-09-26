using System;
using System.Collections.Generic;
using StateMachine.Agents.RTS;
using UnityEngine;

namespace Pathfinder
{
    public class BreadthPathfinder<NodeType, TCoordinateType, TCoordinate> : Pathfinder<NodeType,TCoordinateType, TCoordinate> 
        where NodeType : INode<TCoordinateType>, new()
        where TCoordinateType : IEquatable<TCoordinateType>
        where TCoordinate : ICoordinate<TCoordinateType>, new()
    {
        
        public BreadthPathfinder(ICollection<NodeType> graph)
        {
            this.Graph = graph;
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

        protected override ICollection<INode<TCoordinateType>> GetNeighbors(NodeType node)
        {
            return node.GetNeighbors();
        }

        protected override bool IsBlocked(NodeType node)
        {
            return false;
        }

        protected override int MoveToNeighborCost(NodeType A, NodeType B, RTSAgent.AgentTypes type)
        {
            return 0;
        }

        protected override bool NodesEquals(NodeType A, NodeType B)
        {
            return Equals(A,B);
        }
    }
}
