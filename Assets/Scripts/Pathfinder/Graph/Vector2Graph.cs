using UnityEngine;

namespace Pathfinder.Graph
{
    public class Vector2Graph : Graph<Node<Vector2>, NodeVoronoi, Vector2>
    {
        public Vector2Graph(int x, int y, float cellSize) : base(x, y, cellSize)
        {
        }

        public override void CreateGraph(int x, int y, float cellSize)
        {
            for (var i = 0; i < x; i++)
            for (var j = 0; j < y; j++)
            {
                var node = new NodeVoronoi();
                node.SetCoordinate(i * cellSize, j * cellSize);
                CoordNodes.Add(node);

                var nodeType = new Node<Vector2>();
                nodeType.SetCoordinate(new Vector2(i * cellSize, j * cellSize));
                NodesType.Add(nodeType);
            }
        }
    }
}