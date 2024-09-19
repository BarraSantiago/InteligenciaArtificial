using System;
using System.Collections.Generic;

namespace Pathfinder
{
    public class Vector2IntGraph<NodeType> where NodeType : ICoordinate<NodeType>, INode, new()
    {
        public List<NodeType> nodes = new List<NodeType>();

        public Vector2IntGraph(int x, int y, float cellSize)
        {
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    NodeType node = new NodeType();
                    (node).SetCoordinate(i * cellSize , j * cellSize);
                    nodes.Add(node);
                }
            }
            
            AddNeighbors(cellSize);
        }

        private void AddNeighbors(float cellSize)
        {
            ICollection<NodeType> neighbors = new List<NodeType>();

            for (int i = 0; i < nodes.Count; i++)
            {
                neighbors.Clear();
                for (int j = 0; j < nodes.Count; j++)
                {
                    if(i == j) continue;
                    if ((Approximately(nodes[i].GetX(), nodes[j].GetX()) &&
                         Approximately(Math.Abs(nodes[i].GetY() - nodes[j].GetY()), cellSize)) ||
                        (Approximately(nodes[i].GetY(), nodes[j].GetY()) &&
                         Approximately(Math.Abs(nodes[i].GetX() - nodes[j].GetY()), cellSize)) ||
                        (Approximately(Math.Abs(nodes[i].GetY() - nodes[j].GetY()), cellSize) &&
                         Approximately(Math.Abs(nodes[i].GetX() - nodes[j].GetX()), cellSize)))
                    {
                        neighbors.Add(nodes[j]);
                    }
                }
                nodes[i].GetNeighbors = (ICollection<INode>)neighbors;
            }
        }
        
        public bool Approximately(float a, float b)
        {
            return Math.Abs(a - b) < 1e-6f;
        }
    }
}