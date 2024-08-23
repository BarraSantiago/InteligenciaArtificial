using System.Collections.Generic;
using UnityEngine;

namespace Pathfinder
{
    public class BreadthPathfinder<NodeType> : Pathfinder<NodeType> where NodeType : INode<Vector2Int>, INode
    {
        protected override int Distance(NodeType A, NodeType B)
        {
            throw new System.NotImplementedException();
        }

        protected override ICollection<NodeType> GetNeighbors(NodeType node)
        {
            throw new System.NotImplementedException();
        }

        protected override bool IsBlocked(NodeType node)
        {
            throw new System.NotImplementedException();
        }

        protected override int MoveToNeighborCost(NodeType A, NodeType B)
        {
            throw new System.NotImplementedException();
        }

        protected override bool NodesEquals(NodeType A, NodeType B)
        {
            throw new System.NotImplementedException();
        }
    }
}
