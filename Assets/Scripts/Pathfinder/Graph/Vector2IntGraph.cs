using System.Collections.Generic;
using Game;
using UnityEngine;
using Utils;

namespace Pathfinder
{
    public class Vector2IntGraph<NodeType> where NodeType : ICoordinate<NodeType>, new()
    {
        public List<NodeType> nodes = new List<NodeType>();

        public Vector2IntGraph(int x, int y)
        {
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    NodeType node = new NodeType();
                    (node).SetCoordinate(i, j);
                    nodes.Add(node);
                }
            }
        }

        public Vector2IntGraph(Vector2 mapSize)
        {
            for (int i = 0; i < mapSize.x; i++)
            {
                for (int j = 0; j < mapSize.y; j++)
                {
                    NodeType node = new NodeType();
                    (node).SetCoordinate(i, j);
                    nodes.Add(node);
                }
            }
        }
    }
}