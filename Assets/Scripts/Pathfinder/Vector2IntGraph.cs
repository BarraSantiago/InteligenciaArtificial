using System.Collections.Generic;
using Utils;

namespace Pathfinder
{
    public class Vector2IntGraph<NodeType> where NodeType : INode<Vec2Int>, new()
    {
        public List<NodeType> nodes = new List<NodeType>();

        public Vector2IntGraph(int x, int y)
        {
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    NodeType node = new NodeType();
                    (node).SetCoordinate(new Vec2Int(i, j));
                    nodes.Add(node);
                }
            }
        }
    }
}