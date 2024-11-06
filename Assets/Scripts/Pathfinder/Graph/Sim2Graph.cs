using UnityEngine;

namespace Pathfinder.Graph
{
    public class Sim2Graph : Graph<SimNode<Vector2>, NodeVoronoi, Vector2>
    {
        public Sim2Graph(int x, int y, float cellSize) : base(x, y, cellSize)
        {
        }

        public override void CreateGraph(int x, int y, float cellSize)
        {
            CoordNodes = new NodeVoronoi[x, y];
            for (var i = 0; i < x; i++)
            {
                for (var j = 0; j < y; j++)
                {
                    var node = new NodeVoronoi();
                    node.SetCoordinate(i * cellSize, j * cellSize);
                    CoordNodes[i, j] = node;

                    var nodeType = new SimNode<Vector2>();
                    nodeType.SetCoordinate(new Vector2(i * cellSize, j * cellSize));
                    NodesType.Add(nodeType);
                }
            }
        }
    }
}