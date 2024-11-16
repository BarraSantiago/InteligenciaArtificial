using UnityEngine;
using Utils;

namespace Pathfinder.Graph
{
    public class Sim2Graph : SimGraph<SimNode<IVector>, SimCoordinate, IVector>
    {
        private int MinX => 0;
        private int MaxX => CoordNodes.GetLength(0);
        private int MinY => 0;
        private int MaxY => CoordNodes.GetLength(1);
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
        
        public bool IsWithinGraphBorders(IVector position)
        {
            return position.X >= MinX && position.X <= MaxX &&
                   position.Y >= MinY && position.Y <= MaxY;
        }
    }
}