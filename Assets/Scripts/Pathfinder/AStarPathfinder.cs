using System;
using System.Collections.Generic;


struct Transition <NodeType>
{
    public NodeType to;
    public int cost;
    public int distance;
}

public class AStarPathfinder<NodeType> : Pathfinder<NodeType> where NodeType : INode
{
    private Vector2IntGraph<> graph;
    private Dictionary<NodeType, Transition<NodeType>> transitions = new Dictionary<NodeType, Transition<NodeType>>();
    
    

    protected override int Distance(NodeType A, NodeType B)
    {
        int distance = 0;

        distance += Math.Abs(((INode<(int x, int y)>)A).GetCoordinate().x - ((INode<(int x, int y)>)B).GetCoordinate().x);
        distance += Math.Abs(((INode<(int x, int y)>)A).GetCoordinate().y - ((INode<(int x, int y)>)B).GetCoordinate().y);
        
        return distance;
    }

    protected override ICollection<NodeType> GetNeighbors(NodeType node)
    {
        ICollection<NodeType> neighbors = new List<NodeType>();
        (int x, int y) nodeCoor = ((INode<(int x, int y)>)node).GetCoordinate();
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
        transitions

        return cost;
    }

    protected override bool NodesEquals(NodeType A, NodeType B)
    {
        throw new System.NotImplementedException();
    }
}