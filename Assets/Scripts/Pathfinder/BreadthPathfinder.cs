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
            float distance = 0;
            Node<Vector2> nodeA = A as Node<Vector2>;
            Node<Vector2> nodeB = B as Node<Vector2>;
        
            distance += Math.Abs(nodeA.GetCoordinate().x - nodeB.GetCoordinate().x);
            distance += Math.Abs(nodeA.GetCoordinate().y - nodeB.GetCoordinate().y);

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
