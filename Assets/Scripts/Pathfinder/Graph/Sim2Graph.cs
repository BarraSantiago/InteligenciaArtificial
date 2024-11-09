using UnityEngine;
using Utils;

namespace Pathfinder.Graph
{
    public class Sim2Graph : SimGraph<SimNode<IVector>, SimCoordinate, IVector>
    {
        public Sim2Graph(int x, int y, float cellSize) : base(x, y, cellSize)
        {
        }

        public override void CreateGraph(int x, int y, float cellSize)
        {
            CoordNodes = new SimCoordinate[x, y];
            for (var i = 0; i < x; i++)
            {
                for (var j = 0; j < y; j++)
                {
                    var node = new SimCoordinate();
                    node.SetCoordinate(i * cellSize, j * cellSize);
                    CoordNodes[i, j] = node;

                    var nodeType = new SimNode<IVector>();
                    nodeType.SetCoordinate(new MyVector(i * cellSize, j * cellSize));
                    NodesType[i, j] = nodeType;
                }
            }
        }
    }
}