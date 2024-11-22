using System;
using System.Collections.Generic;

namespace Pathfinder.Graph
{
    public abstract class Graph<TNodeType, TCoordinateNode, TCoordinateType>
        where TNodeType : INode<TCoordinateType>, new()
        where TCoordinateNode : ICoordinate<TCoordinateType>, new()
        where TCoordinateType : IEquatable<TCoordinateType>
    {
        public static List<RTSNode<TCoordinateType>> mines = new();

        public static TCoordinateNode MapDimensions;
        public static TCoordinateNode OriginPosition;
        public static float CellSize;
        public TCoordinateNode[,] CoordNodes;
        public readonly List<TNodeType> NodesType = new();

        public Graph(int x, int y, float cellSize)
        {
            MapDimensions = new TCoordinateNode();
            MapDimensions.SetCoordinate(x, y);
            CellSize = cellSize;

            CreateGraph(x, y, cellSize);

            AddNeighbors(cellSize);
        }

        public abstract void CreateGraph(int x, int y, float cellSize);

        private void AddNeighbors(float cellSize)
        {
            List<INode<TCoordinateType>> neighbors = new List<INode<TCoordinateType>>();

            for (int i = 0; i < CoordNodes.GetLength(0); i++)
            {
                for (int j = 0; j < CoordNodes.GetLength(1); j++)
                {
                    neighbors.Clear();
                    for (int k = 0; k < CoordNodes.GetLength(0); k++)
                    {
                        for (int l = 0; l < CoordNodes.GetLength(1); l++)
                        {
                            if (i == k && j == l) continue;

                            bool isNeighbor =
                                (Approximately(CoordNodes[i, j].GetX(), CoordNodes[k, l].GetX()) &&
                                 Approximately(Math.Abs(CoordNodes[i, j].GetY() - CoordNodes[k, l].GetY()), cellSize)) ||
                                (Approximately(CoordNodes[i, j].GetY(), CoordNodes[k, l].GetY()) &&
                                 Approximately(Math.Abs(CoordNodes[i, j].GetX() - CoordNodes[k, l].GetX()), cellSize));

                            if (isNeighbor) neighbors.Add(NodesType[k * CoordNodes.GetLength(1) + l]);
                        }
                    }

                    NodesType[i * CoordNodes.GetLength(1) + j].SetNeighbors(new List<INode<TCoordinateType>>(neighbors));
                }
            }
        }
        public bool Approximately(float a, float b)
        {
            return Math.Abs(a - b) < 1e-6f;
        }
    }
}