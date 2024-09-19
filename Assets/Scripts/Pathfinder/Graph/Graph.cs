using System;
using System.Collections.Generic;

namespace Pathfinder
{
    public class Graph<NodeType, CoordinateNode, CoordinateType>
        where NodeType : INode, INode<CoordinateType>, new()
        where CoordinateNode : ICoordinate<CoordinateType>, new()
        where CoordinateType : IEquatable<CoordinateType>

    {
        public List<CoordinateNode> coordNodes = new List<CoordinateNode>();
        public List<NodeType> nodesType = new List<NodeType>();

        public Graph(int x, int y, float cellSize)
        {
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    CoordinateNode node = new CoordinateNode();
                    (node).SetCoordinate(i * cellSize, j * cellSize);
                    coordNodes.Add(node);
                    
                    NodeType nodeType = new NodeType();
                    nodeType.SetCoordinate(i * cellSize, j * cellSize);
                    nodesType.Add(nodeType);
                }
            }

            AddNeighbors(cellSize);
        }

        private void AddNeighbors(float cellSize)
        {
            ICollection<NodeType> neighbors = new List<NodeType>();

            for (int i = 0; i < coordNodes.Count; i++)
            {
                neighbors.Clear();
                for (int j = 0; j < coordNodes.Count; j++)
                {
                    if (i == j) continue;
                    if ((Approximately(coordNodes[i].GetX(), coordNodes[j].GetX()) &&
                         Approximately(Math.Abs(coordNodes[i].GetY() - coordNodes[j].GetY()), cellSize)) ||
                        (Approximately(coordNodes[i].GetY(), coordNodes[j].GetY()) &&
                         Approximately(Math.Abs(coordNodes[i].GetX() - coordNodes[j].GetY()), cellSize)) ||
                        (Approximately(Math.Abs(coordNodes[i].GetY() - coordNodes[j].GetY()), cellSize) &&
                         Approximately(Math.Abs(coordNodes[i].GetX() - coordNodes[j].GetX()), cellSize)))
                    {
                        neighbors.Add(nodesType[j]);
                    }
                }
                
                nodesType[i].GetNeighbors = (ICollection<INode>)neighbors;
            }
        }

        public bool Approximately(float a, float b)
        {
            return Math.Abs(a - b) < 1e-6f;
        }
    }
}