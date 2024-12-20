﻿using UnityEngine;

namespace Pathfinder.Graph
{
    public class Vector2Graph : Graph<RTSNode<Vector2>, NodeVoronoi, Vector2>
    {
        public Vector2Graph(int x, int y, float cellSize) : base(x, y, cellSize)
        {
        }

        public override void CreateGraph(int x, int y, float cellSize)
        {
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    NodeVoronoi node = new NodeVoronoi();
                    node.SetCoordinate(i * cellSize, j * cellSize);
                    CoordNodes[i, j] = node;

                    RTSNode<Vector2> nodeType = new RTSNode<Vector2>();
                    nodeType.SetCoordinate(new Vector2(i * cellSize, j * cellSize));
                    NodesType.Add(nodeType);
                }
            }
        }
    }
}