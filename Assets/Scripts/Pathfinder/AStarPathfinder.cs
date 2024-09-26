using System;
using System.Collections.Generic;
using StateMachine.Agents.RTS;

namespace Pathfinder
{
    public class AStarPathfinder<NodeType, CoordinateType, TCoordinate> : Pathfinder<NodeType, CoordinateType, TCoordinate>
        where NodeType : INode, INode<CoordinateType>, new()
        where CoordinateType : IEquatable<CoordinateType>
        where TCoordinate : ICoordinate<CoordinateType>, new()
    {
        public AStarPathfinder(ICollection<NodeType> graph)
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

        protected override ICollection<INode<CoordinateType>> GetNeighbors(NodeType node)
        {
            return node.GetNeighbors();
        }
        
        protected override bool IsBlocked(NodeType node)
        {
            return node.GetNodeType() == Pathfinder.NodeType.Blocked;
        }

        protected override int MoveToNeighborCost(NodeType A, NodeType B, RTSAgent.AgentTypes type)
        {
            if (!GetNeighbors(A).Contains(B))
            {
                throw new InvalidOperationException("B node has to be a neighbor.");
            }

            int cost = 0;

            switch (type)
            {
                case RTSAgent.AgentTypes.Miner:
                    if (B.GetNodeType() == Pathfinder.NodeType.Gravel) cost += 2;
                    break;
                case RTSAgent.AgentTypes.Caravan:
                    if (B.GetNodeType() == Pathfinder.NodeType.Forest) cost += 2;
                    break;
                default:
                cost = 0;
                    break;
            }

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